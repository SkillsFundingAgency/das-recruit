using Esfa.Recruit.Vacancies.Client.Application.Exceptions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using System;
using System.Threading.Tasks;
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
        void CheckAuthorisedAccess(Vacancy vacancy, string employerAccountId, bool vacancySharedByProvider = false);
        PartOnePageInfoViewModel GetPartOnePageInfo(Vacancy vacancy);
        Task<ApplicationReview> GetAuthorisedApplicationReviewAsync(ApplicationReviewRouteModel rm, bool vacancySharedByProvider = false);
        Task UpdateEmployerProfile(VacancyEmployerInfoModel employerInfoModel, EmployerProfile employerProfile, Address address, VacancyUser user);
    }
    
    public class Utility(IRecruitVacancyClient vacancyClient) : VacancyTaskListStatusService, IUtility
    {
        public async Task<Vacancy> GetAuthorisedVacancyForEditAsync(VacancyRouteModel vrm, string routeName)
        {
            var vacancy = await GetAuthorisedVacancyAsync(vrm, routeName);
            CheckCanEdit(vacancy);
            return vacancy;
        }

        public async Task<Vacancy> GetAuthorisedVacancyAsync(VacancyRouteModel vrm, string routeName)
        {
            var vacancy = await vacancyClient.GetVacancyAsync(vrm.VacancyId);
            CheckAuthorisedAccess(vacancy, vrm.EmployerAccountId);
            return vacancy;
        }

        public void CheckCanEdit(Vacancy vacancy)
        {
            if (!vacancy.CanEmployerEdit)
                throw new InvalidStateException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));
        }

        public void CheckAuthorisedAccess(Vacancy vacancy, string employerAccountId, bool vacancySharedByProvider = false)
        {
            if (!vacancy.EmployerAccountId.Equals(employerAccountId, StringComparison.OrdinalIgnoreCase))
                throw new AuthorisationException(string.Format(ExceptionMessages.VacancyUnauthorisedAccess, employerAccountId, vacancy.EmployerAccountId, vacancy.Title, vacancy.Id));
            if (!vacancy.CanEmployerAndProviderCollabarate && vacancy.OwnerType != OwnerType.Employer && !vacancySharedByProvider)
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

        public async Task<ApplicationReview> GetAuthorisedApplicationReviewAsync(ApplicationReviewRouteModel rm, bool vacancySharedByProvider = false)
        {
            var applicationReview = vacancyClient.GetApplicationReviewAsync(rm.ApplicationReviewId);
            var vacancy = vacancyClient.GetVacancyAsync(rm.VacancyId);

            await Task.WhenAll(applicationReview, vacancy);

            applicationReview.Result.AdditionalQuestion1 = vacancy.Result.AdditionalQuestion1;
            applicationReview.Result.AdditionalQuestion2 = vacancy.Result.AdditionalQuestion2;
            applicationReview.Result.VacancyTitle = vacancy.Result.Title;
            try
            {
                CheckAuthorisedAccess(vacancy.Result, rm.EmployerAccountId, vacancySharedByProvider);
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
                await vacancyClient.UpdateEmployerProfileAsync(employerProfile, user);
            }
        }
    }
}
