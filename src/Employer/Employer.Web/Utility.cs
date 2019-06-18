﻿using Esfa.Recruit.Vacancies.Client.Application.Exceptions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Exceptions;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

namespace Esfa.Recruit.Employer.Web
{
    public static class Utility
    {
        public static async Task<Vacancy> GetAuthorisedVacancyForEditAsync(IEmployerVacancyClient client, IRecruitVacancyClient vacancyClient, VacancyRouteModel vrm, string routeName)
        {
            var vacancy = await GetAuthorisedVacancyAsync(vacancyClient, vrm, routeName);

            CheckCanEdit(vacancy);

            return vacancy;
        }

        public static async Task<Vacancy> GetAuthorisedVacancyAsync(IRecruitVacancyClient client, VacancyRouteModel vrm, string routeName)
        {
            var vacancy = await client.GetVacancyAsync(vrm.VacancyId);

            CheckAuthorisedAccess(vacancy, vrm.EmployerAccountId);

            CheckRouteIsValidForVacancy(vacancy, routeName);

            return vacancy;
        }

        private static void CheckCanEdit(Vacancy vacancy)
        {
            if (!vacancy.CanEdit)
                throw new InvalidStateException(string.Format(ErrorMessages.VacancyNotAvailableForEditing,
                    vacancy.Title));
        }

        public static void CheckAuthorisedAccess(Vacancy vacancy, string employerAccountId)
        {
            if (!vacancy.EmployerAccountId.Equals(employerAccountId, StringComparison.OrdinalIgnoreCase))
                throw new AuthorisationException(string.Format(ExceptionMessages.VacancyUnauthorisedAccess, employerAccountId, vacancy.EmployerAccountId, vacancy.Title, vacancy.Id));
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
                redirectRoute, new VacancyRouteModel{ EmployerAccountId = vacancy.EmployerAccountId, VacancyId = vacancy.Id });
        }

        public static IList<string> GetValidRoutesForVacancy(Vacancy vacancy)
        {
            var validRoutes = new List<string>();

            validRoutes.AddRange(new [] {RouteNames.Title_Post, RouteNames.Title_Get});
            if (string.IsNullOrWhiteSpace(vacancy.Title))
                return validRoutes;

            validRoutes.AddRange(new[] {
                RouteNames.TrainingProvider_Confirm_Post, RouteNames.TrainingProvider_Confirm_Get, RouteNames.TrainingProvider_Select_Post, RouteNames.TrainingProvider_Select_Get,
                RouteNames.NumberOfPositions_Post, RouteNames.NumberOfPositions_Get });

            if(vacancy.TrainingProvider == null && string.IsNullOrWhiteSpace(vacancy.NumberOfPositions?.ToString()))
            {
                //Move Training Provider Get to last valid route for resuming
                validRoutes.Remove(RouteNames.TrainingProvider_Select_Get);
                validRoutes.Add(RouteNames.TrainingProvider_Select_Get);
            }
            
            if (string.IsNullOrWhiteSpace(vacancy.NumberOfPositions?.ToString()))
                return validRoutes;

            validRoutes.AddRange(new[] 
            {
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

            validRoutes.AddRange(new[] {RouteNames.Training_Post, RouteNames.Training_Get});
            if (string.IsNullOrWhiteSpace(vacancy.ProgrammeId))
                return validRoutes;

            validRoutes.AddRange(new[] { RouteNames.Wage_Post, RouteNames.Wage_Get});
            if (vacancy.Wage?.WageType == null)
                return validRoutes;

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
                   vacancy.EmployerContact != null ||
                   vacancy.Qualifications != null ||
                   vacancy.Skills != null ||
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
            var vacancy = await client.GetVacancyAsync(rm.VacancyId);
            try
            {
                CheckAuthorisedAccess(vacancy, rm.EmployerAccountId);
                return applicationReview;
            }
            catch (Exception)
            {
                throw new AuthorisationException(string.Format(ExceptionMessages.ApplicationReviewUnauthorisedAccess, rm.EmployerAccountId,
                    vacancy.EmployerAccountId, rm.ApplicationReviewId, vacancy.Id));
            }
        }
    }
}
