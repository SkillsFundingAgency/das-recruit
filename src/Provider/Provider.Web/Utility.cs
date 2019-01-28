﻿using Esfa.Recruit.Vacancies.Client.Application.Exceptions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Exceptions;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Provider.Web.ViewModels.Part1;
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

            CheckRouteIsValidForVacancy(vacancy, routeName);

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
                throw new AuthorisationException(string.Format(ExceptionMessages.VacancyUnauthorisedAccess, ukprn, vacancy.TrainingProvider.Ukprn, vacancy.Title, vacancy.Id));
        }

        public static void CheckRouteIsValidForVacancy(Vacancy vacancy, string currentRouteName)
        {
            var validRoutes = GetValidRoutesForVacancy(vacancy);

            if (validRoutes == null || validRoutes.Contains(currentRouteName))
            {
                return;
            }

            var redirectRoute = validRoutes.Last();
            
            throw new InvalidRouteForVacancyException(string.Format(RecruitWebExceptionMessages.RouteNotValidForVacancy, currentRouteName, redirectRoute),
                redirectRoute, new VacancyRouteModel{ Ukprn = vacancy.TrainingProvider.Ukprn.GetValueOrDefault(), VacancyId = vacancy.Id });
        }

        public static IList<string> GetValidRoutesForVacancy(Vacancy vacancy)
        {
            var validRoutes = new List<string>();

            validRoutes.AddRange(new [] {RouteNames.Title_Post, RouteNames.Title_Get});
            if (string.IsNullOrWhiteSpace(vacancy.Title))
                return validRoutes;

            // validRoutes.AddRange(new[] { RouteNames.ShortDescription_Post, RouteNames.ShortDescription_Get });
            // if (string.IsNullOrWhiteSpace(vacancy.ShortDescription))
            //     return validRoutes;

            // validRoutes.AddRange(new[] { RouteNames.Provider_Post, RouteNames.Provider_Get});
            // if (string.IsNullOrWhiteSpace(vacancy.ProviderLocation?.Postcode))
            //     return validRoutes;
            
            // validRoutes.AddRange(new[] {RouteNames.LegalEntityAgreement_SoftStop_Get, RouteNames.Training_Post, RouteNames.Training_Get});
            // if (string.IsNullOrWhiteSpace(vacancy.ProgrammeId))
            //     return validRoutes;

            // validRoutes.AddRange(new[] { RouteNames.Wage_Post, RouteNames.Wage_Get});
            // if (vacancy.Wage?.WageType == null)
            //     return validRoutes;

            return null;
        }

        public static bool VacancyHasCompletedPartOne(Vacancy vacancy)
        {
            return GetValidRoutesForVacancy(vacancy) == null;
        }

        public static bool VacancyHasStartedPartTwo(Vacancy vacancy)
        {
            return !string.IsNullOrWhiteSpace(vacancy.EmployerDescription) ||
                   vacancy.ApplicationMethod != null ||
                   !string.IsNullOrWhiteSpace(vacancy.ThingsToConsider) ||
                   vacancy.ProviderContact != null ||
                   vacancy.Qualifications != null ||
                   vacancy.Skills != null ||
                   vacancy.TrainingProvider != null ||
                   !string.IsNullOrWhiteSpace(vacancy.Description);
        }

        public static PartOnePageInfoViewModel GetPartOnePageInfo(Vacancy vacancy)
        {
            return new PartOnePageInfoViewModel
            {
                HasCompletedPartOne = VacancyHasCompletedPartOne(vacancy),
                HasStartedPartTwo = VacancyHasStartedPartTwo(vacancy)
            };
        }

        public static async Task<ApplicationReview> GetAuthorisedApplicationReviewAsync(IRecruitVacancyClient client, ApplicationReviewRouteModel rm)
        {
            var applicationReview = await client.GetApplicationReviewAsync(rm.ApplicationReviewId);

            //TODO: this needs changing when we implement application review story
            if (applicationReview.EmployerAccountId == rm.Ukprn.ToString()) // needs to be reviewed
            {
                return applicationReview;
            }

            throw new AuthorisationException(string.Format(ExceptionMessages.ApplicationReviewUnauthorisedAccess, rm.Ukprn, applicationReview.EmployerAccountId, applicationReview.Id, applicationReview.VacancyReference)); // needs to be reviewed
        }
    }
}
