using Esfa.Recruit.Vacancies.Client.Application.Exceptions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Exceptions;
using Esfa.Recruit.Employer.Web.Models;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Shared.Web.Models;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.VacancySummariesProvider;

namespace Esfa.Recruit.Employer.Web
{
    public interface IUtility : IVacancyTaskListStatusService
    {
        Task<Vacancy> GetAuthorisedVacancyForEditAsync(VacancyRouteModel vrm, string routeName);
        Task<Vacancy> GetAuthorisedVacancyAsync(VacancyRouteModel vrm, string routeName);
        void CheckCanEdit(Vacancy vacancy);
        void CheckAuthorisedAccess(Vacancy vacancy, string employerAccountId);
        PartOnePageInfoViewModel GetPartOnePageInfo(Vacancy vacancy);
        Task<ApplicationReview> GetAuthorisedApplicationReviewAsync(ApplicationReviewRouteModel rm);

        Task UpdateEmployerProfile(VacancyEmployerInfoModel employerInfoModel, 
            EmployerProfile employerProfile, Address address, VacancyUser user);
    }
    
    public class Utility : VacancyTaskListStatusService, IUtility
    {
        private readonly IRecruitVacancyClient _vacancyClient;

        public Utility (IRecruitVacancyClient vacancyClient)
        {
            _vacancyClient = vacancyClient;
        }
        
        public async Task<Vacancy> GetAuthorisedVacancyForEditAsync(VacancyRouteModel vrm, string routeName)
        {
            var vacancy = await GetAuthorisedVacancyAsync(vrm, routeName);

            CheckCanEdit(vacancy);

            return vacancy;
        }

        public async Task<Vacancy> GetAuthorisedVacancyAsync(VacancyRouteModel vrm, string routeName)
        {
            var vacancy = await _vacancyClient.GetVacancyAsync(vrm.VacancyId);

            CheckAuthorisedAccess(vacancy, vrm.EmployerAccountId);

            return vacancy;
        }

        public void CheckCanEdit(Vacancy vacancy)
        {
            if (!vacancy.CanEmployerEdit)
                throw new InvalidStateException(string.Format(ErrorMessages.VacancyNotAvailableForEditing,
                    vacancy.Title));
        }

        public void CheckAuthorisedAccess(Vacancy vacancy, string employerAccountId)
        {
            if (!vacancy.EmployerAccountId.Equals(employerAccountId, StringComparison.OrdinalIgnoreCase))
                throw new AuthorisationException(string.Format(ExceptionMessages.VacancyUnauthorisedAccess, employerAccountId, vacancy.EmployerAccountId, vacancy.Title, vacancy.Id));
            if (!vacancy.CanEmployerAndProviderCollabarate && vacancy.OwnerType != OwnerType.Employer)
                throw new AuthorisationException(string.Format(ExceptionMessages.UserIsNotTheOwner, OwnerType.Employer));
        }

        public bool VacancyHasCompletedPartOne(Vacancy vacancy)
        {
            return vacancy.ApplicationMethod != null;
        }

        public bool VacancyHasStartedPartTwo(Vacancy vacancy)
        {
            return !string.IsNullOrWhiteSpace(vacancy.EmployerDescription) ||
                   vacancy.ApplicationMethod != null ||
                   !string.IsNullOrWhiteSpace(vacancy.ThingsToConsider) ||
                   vacancy.EmployerContact != null ||
                   vacancy.Qualifications != null ||
                   vacancy.Skills != null ||
                   !string.IsNullOrWhiteSpace(vacancy.Description) ||
                !string.IsNullOrWhiteSpace(vacancy.ShortDescription);
        }

        public PartOnePageInfoViewModel GetPartOnePageInfo(Vacancy vacancy)
        {
            return new PartOnePageInfoViewModel
            {
                HasCompletedPartOne = VacancyHasCompletedPartOne(vacancy),
                HasStartedPartTwo = VacancyHasStartedPartTwo(vacancy)
            };
        }

        public async Task<ApplicationReview> GetAuthorisedApplicationReviewAsync(ApplicationReviewRouteModel rm)
        {
            var applicationReview = _vacancyClient.GetApplicationReviewAsync(rm.ApplicationReviewId);
            var vacancy = _vacancyClient.GetVacancyAsync(rm.VacancyId);

            await Task.WhenAll(applicationReview, vacancy);
            
            try
            {
                CheckAuthorisedAccess(vacancy.Result, rm.EmployerAccountId);
                return applicationReview.Result;
            }
            catch (Exception)
            {
                throw new AuthorisationException(string.Format(ExceptionMessages.ApplicationReviewUnauthorisedAccess, rm.EmployerAccountId,
                    vacancy.Result.EmployerAccountId, rm.ApplicationReviewId, vacancy.Result.Id));
            }
        }
        
        public async Task UpdateEmployerProfile(VacancyEmployerInfoModel employerInfoModel, 
            EmployerProfile employerProfile, Address address, VacancyUser user)
        {
            var updateProfile = false;
            if (string.IsNullOrEmpty(employerProfile.AccountLegalEntityPublicHashedId) && !string.IsNullOrEmpty(employerInfoModel?.AccountLegalEntityPublicHashedId)) 
            {
                updateProfile = true;
                employerProfile.AccountLegalEntityPublicHashedId = employerInfoModel.AccountLegalEntityPublicHashedId;
            }
            if (employerInfoModel != null && employerInfoModel.EmployerIdentityOption == EmployerIdentityOption.NewTradingName)
            {
                updateProfile = true;
                employerProfile.TradingName = employerInfoModel.NewTradingName;
            }
            if (address != null)
            {
                updateProfile = true;
                employerProfile.OtherLocations.Add(address);
            }
            if (updateProfile)    
            {
                await _vacancyClient.UpdateEmployerProfileAsync(employerProfile, user);
            }
        }
    }
}
