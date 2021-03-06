﻿using System;
using Esfa.Recruit.Vacancies.Client.Application.Exceptions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Exceptions;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

namespace Esfa.Recruit.Provider.Web
{
    public static class Utility
    {
        public static async Task<Vacancy> GetAuthorisedVacancyForEditAsync(IProviderVacancyClient client, IRecruitVacancyClient vacancyClient, VacancyRouteModel vrm, string routeName)
        {
            var vacancy = await GetAuthorisedVacancyAsync(client, vacancyClient, vrm, routeName);

            CheckCanEdit(vacancy);

            return vacancy;
        }

        public static async Task<Vacancy> GetAuthorisedVacancyAsync(IProviderVacancyClient client, IRecruitVacancyClient vacancyClient, VacancyRouteModel vrm, string routeName)
        {
            var vacancy = await vacancyClient.GetVacancyAsync(vrm.VacancyId.GetValueOrDefault());

            CheckAuthorisedAccess(vacancy, vrm.Ukprn);

            CheckRouteIsValidForVacancy(vacancy, routeName, vrm);

            return vacancy;
        }

        private static void CheckCanEdit(Vacancy vacancy)
        {
            if (!vacancy.CanEdit)
                throw new InvalidStateException(string.Format(ErrorMessages.VacancyNotAvailableForEditing,
                    vacancy.Title));
        }

        public static void CheckAuthorisedAccess(Vacancy vacancy, long ukprn)
        {
            if (vacancy.TrainingProvider.Ukprn.Value != ukprn)
                throw new AuthorisationException(string.Format(ExceptionMessages.VacancyUnauthorisedAccessForProvider, ukprn, vacancy.TrainingProvider.Ukprn, vacancy.Title, vacancy.Id));
            if (vacancy.OwnerType != OwnerType.Provider)
                throw new AuthorisationException(string.Format(ExceptionMessages.UserIsNotTheOwner, OwnerType.Provider));
        }

        public static void CheckRouteIsValidForVacancy(Vacancy vacancy, string currentRouteName, VacancyRouteModel vrm)
        {
            var validRoutes = GetPermittedRoutesForVacancy(vacancy);

            if (validRoutes == null || validRoutes.Contains(currentRouteName))
            {
                return;
            }

            var redirectRoute = validRoutes.Last();
            
            throw new InvalidRouteForVacancyException(string.Format(RecruitWebExceptionMessages.RouteNotValidForVacancy, currentRouteName, redirectRoute),
                redirectRoute, vrm);
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
        public static IList<string> GetPermittedRoutesForVacancy(Vacancy vacancy)
        {
            var validRoutes = new List<string>();

            validRoutes.AddRange(new [] {RouteNames.Title_Post, RouteNames.Title_Get});
            if (string.IsNullOrWhiteSpace(vacancy.Title))
                return validRoutes;

            validRoutes.AddRange(new[]
            {
                RouteNames.Training_Confirm_Post,
                RouteNames.Training_Confirm_Get,
                RouteNames.Training_Post,
                RouteNames.Training_Get
            });
            if (string.IsNullOrWhiteSpace(vacancy.ProgrammeId))
                return validRoutes;

            validRoutes.AddRange(new[] {
                RouteNames.NumberOfPositions_Post,
                RouteNames.NumberOfPositions_Get });

            if (!vacancy.NumberOfPositions.HasValue)
                return validRoutes;

           validRoutes.AddRange(new[] 
            {
                RouteNames.Location_Get, 
                RouteNames.Location_Post,
                RouteNames.EmployerName_Post, 
                RouteNames.EmployerName_Get, 
                RouteNames.LegalEntity_Post, 
                RouteNames.LegalEntity_Get
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

        public static bool VacancyHasCompletedPartOne(Vacancy vacancy)
        {
            return GetPermittedRoutesForVacancy(vacancy) == null;
        }

        public static bool VacancyHasStartedPartTwo(Vacancy vacancy)
        {
            return !string.IsNullOrWhiteSpace(vacancy.EmployerDescription) ||
                   vacancy.ApplicationMethod != null ||
                   !string.IsNullOrWhiteSpace(vacancy.ThingsToConsider) ||
                   vacancy.ProviderContact != null ||
                   vacancy.Qualifications != null ||
                   vacancy.Skills != null ||
                   !string.IsNullOrWhiteSpace(vacancy.Description) ||
                   !string.IsNullOrWhiteSpace(vacancy.ShortDescription);
        }

        public static PartOnePageInfoViewModel GetPartOnePageInfo(Vacancy vacancy)
        {
            return new PartOnePageInfoViewModel
            {
                HasCompletedPartOne = VacancyHasCompletedPartOne(vacancy),
                HasStartedPartTwo = VacancyHasStartedPartTwo(vacancy)
            };
        }

        public static async Task<ApplicationReview> GetAuthorisedApplicationReviewAsync(IRecruitVacancyClient vacancyClient, ApplicationReviewRouteModel rm)
        {
            var applicationReview = await vacancyClient.GetApplicationReviewAsync(rm.ApplicationReviewId);
            var vacancy = await vacancyClient.GetVacancyAsync(rm.VacancyId.GetValueOrDefault());
            try
            {
                CheckAuthorisedAccess(vacancy, rm.Ukprn);
                return applicationReview;
            }
            catch (Exception)
            {
                throw new AuthorisationException(string.Format(ExceptionMessages.ApplicationReviewUnauthorisedAccessForProvider, rm.Ukprn, 
                    vacancy.TrainingProvider.Ukprn, rm.ApplicationReviewId,vacancy.Id));
            }                    
        }
    }
}
