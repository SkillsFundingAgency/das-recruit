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
        void CheckAuthorisedAccess(Vacancy vacancy, string employerAccountId, bool vacancySharedByProvider = false);
        void CheckRouteIsValidForVacancy(Vacancy vacancy, string currentRouteName);

        /// <summary>
        /// Returns a list of routes the user may access based on the current
        /// state of the vacancy.
        /// </summary>
        /// <param name="vacancy"></param>
        /// <returns>
        ///  - null if section 1 of the wizard is complete
        ///  - otherwise a list of accessible routes, where the last entry is the page to start the user on when editing the vacancy
        /// </returns>
        IList<string> GetPermittedRoutesForVacancy(Vacancy vacancy);

        bool VacancyHasCompletedPartOne(Vacancy vacancy);
        bool VacancyHasStartedPartTwo(Vacancy vacancy);
        PartOnePageInfoViewModel GetPartOnePageInfo(Vacancy vacancy);
        Task<ApplicationReview> GetAuthorisedApplicationReviewAsync(ApplicationReviewRouteModel rm, bool vacancySharedByProvider = false);

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

            CheckRouteIsValidForVacancy(vacancy, routeName);

            return vacancy;
        }

        public void CheckCanEdit(Vacancy vacancy)
        {
            if (!vacancy.CanEmployerEdit)
                throw new InvalidStateException(string.Format(ErrorMessages.VacancyNotAvailableForEditing,
                    vacancy.Title));
        }

        public void CheckAuthorisedAccess(Vacancy vacancy, string employerAccountId, bool vacancySharedByProvider = false)
        {
            if (!vacancy.EmployerAccountId.Equals(employerAccountId, StringComparison.OrdinalIgnoreCase))
                throw new AuthorisationException(string.Format(ExceptionMessages.VacancyUnauthorisedAccess, employerAccountId, vacancy.EmployerAccountId, vacancy.Title, vacancy.Id));
            if (!vacancy.CanEmployerAndProviderCollabarate && vacancy.OwnerType != OwnerType.Employer && !vacancySharedByProvider)
                throw new AuthorisationException(string.Format(ExceptionMessages.UserIsNotTheOwner, OwnerType.Employer));
        }

        public void CheckRouteIsValidForVacancy(Vacancy vacancy, string currentRouteName)
        {
            var validRoutes = GetPermittedRoutesForVacancy(vacancy);

            if (validRoutes == null || validRoutes.Contains(currentRouteName))
            {
                return;
            }

            var redirectRoute = validRoutes.Last();
            
            throw new InvalidRouteForVacancyException(string.Format(RecruitWebExceptionMessages.RouteNotValidForVacancy, currentRouteName, redirectRoute),
                redirectRoute, new VacancyRouteModel{ EmployerAccountId = vacancy.EmployerAccountId, VacancyId = vacancy.Id });
        }

        /// <summary>
        /// Returns a list of routes the user may access based on the current
        /// state of the vacancy.
        /// </summary>
        /// <param name="vacancy"></param>
        /// <returns>
        ///  - null if section 1 of the wizard is complete
        ///  - otherwise a list of accessible routes, where the last entry is the page to start the user on when editing the vacancy
        /// </returns>
        public IList<string> GetPermittedRoutesForVacancy(Vacancy vacancy)
        {
            var validRoutes = new List<string>();

            validRoutes.AddRange(new [] {RouteNames.Title_Post, RouteNames.Title_Get});

            validRoutes.AddRange( new[]
            {
                RouteNames.EmployerTaskListGet,
                RouteNames.EmployerCheckYourAnswersGet,
                RouteNames.Employer_Post,
                RouteNames.Employer_Get,
                RouteNames.EmployerName_Post,
                RouteNames.EmployerName_Get,
            });

            if (string.IsNullOrWhiteSpace(vacancy.Title))
                return validRoutes;

            validRoutes.AddRange(new[]
            {
                RouteNames.Training_Help_Get,
                RouteNames.Training_First_Time_Post,
                RouteNames.Training_First_Time_Get,
                RouteNames.Training_Confirm_Post,
                RouteNames.Training_Confirm_Get,
                RouteNames.Training_Post,
                RouteNames.Training_Get
            });
            if (string.IsNullOrWhiteSpace(vacancy.ProgrammeId))
                return validRoutes;

            validRoutes.AddRange(new[] {
                RouteNames.TrainingProvider_Confirm_Post,
                RouteNames.TrainingProvider_Confirm_Get,
                RouteNames.TrainingProvider_Select_Post,
                RouteNames.TrainingProvider_Select_Get,
                RouteNames.NumberOfPositions_Post,
                RouteNames.NumberOfPositions_Get
                });

            if (vacancy.TrainingProvider == null && string.IsNullOrWhiteSpace(vacancy.NumberOfPositions?.ToString()))
            {
                //Move Training Provider Get to last valid route for resuming
                validRoutes.Remove(RouteNames.TrainingProvider_Select_Get);
                validRoutes.Add(RouteNames.TrainingProvider_Select_Get);
            }
            
            validRoutes.AddRange(new []
            {
                RouteNames.ShortDescription_Get,
                RouteNames.ShortDescription_Post,
                RouteNames.VacancyDescription_Index_Post,
                RouteNames.VacancyDescription_Index_Get,
                RouteNames.VacancyHowTheApprenticeWillTrain_Index_Get,
                RouteNames.VacancyHowTheApprenticeWillTrain_Index_Post,
                RouteNames.VacancyWorkDescription_Index_Post,
                RouteNames.VacancyWorkDescription_Index_Get,
                RouteNames.Dates_Post, 
                RouteNames.Dates_Get,
                RouteNames.Duration_Post, 
                RouteNames.Duration_Get,
                RouteNames.Wage_Post, 
                RouteNames.Wage_Get,
                RouteNames.CustomWage_Post,
                RouteNames.CustomWage_Get,
                RouteNames.Skills_Post,
                RouteNames.Skills_Get,
                RouteNames.Qualification_Add_Post,
                RouteNames.Qualification_Delete_Post,
                RouteNames.Qualification_Edit_Post,
                RouteNames.Qualification_Edit_Get,
                RouteNames.Qualification_Add_Get,
                RouteNames.Qualifications_Get,
                RouteNames.FutureProspects_Get,
                RouteNames.FutureProspects_Post,
                RouteNames.Considerations_Post,
                RouteNames.Considerations_Get,
                
                RouteNames.AboutEmployer_Post,
                RouteNames.AboutEmployer_Get,
                RouteNames.EmployerContactDetails_Post,
                RouteNames.EmployerContactDetails_Get,
                RouteNames.ApplicationProcess_Post,
                RouteNames.ApplicationProcess_Get,
                RouteNames.AdditionalQuestions_Get,
                RouteNames.AdditionalQuestions_Post,

                RouteNames.SetCompetitivePayRate_Get,
                RouteNames.SetCompetitivePayRate_Post,

                RouteNames.AddExtraInformation_Get,
                RouteNames.AddExtraInformation_Post,

                RouteNames.LegalEntityAgreement_SoftStop_Get,
                RouteNames.Location_Get, 
                RouteNames.Location_Post,
                RouteNames.EmployerName_Post, 
                RouteNames.EmployerName_Get, 
                RouteNames.Employer_Post, 
                RouteNames.Employer_Get
            });

            if (string.IsNullOrWhiteSpace(vacancy.LegalEntityName) 
                || vacancy.EmployerNameOption == null 
                || string.IsNullOrWhiteSpace(vacancy.EmployerLocation?.Postcode))
                return validRoutes;

            validRoutes.AddRange(new[] { RouteNames.Dates_Post, RouteNames.Dates_Get });
            if (vacancy.StartDate == null)
                return validRoutes;

            validRoutes.AddRange(new[] { RouteNames.Duration_Post, RouteNames.Duration_Get });
            if (vacancy.Wage?.Duration == null)
                return validRoutes;

            validRoutes.AddRange(new[] { RouteNames.Wage_Post, RouteNames.Wage_Get});
            if (vacancy.Wage?.WageType == null)
                return validRoutes;

            return null;
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
            var applicationReview = _vacancyClient.GetApplicationReviewAsync(rm.ApplicationReviewId);
            var vacancy = _vacancyClient.GetVacancyAsync(rm.VacancyId);

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
                await _vacancyClient.UpdateEmployerProfileAsync(employerProfile, user);
            }
        }
    }
}
