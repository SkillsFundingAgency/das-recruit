using Esfa.Recruit.Vacancies.Client.Application.Exceptions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Exceptions;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.Orchestrators.Part1;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Esfa.Recruit.Employer.Web
{
    public static class Utility
    {
        public static async Task<Vacancy> GetAuthorisedVacancyForEditAsync(IEmployerVacancyClient client, VacancyRouteModel vrm, string routeName)
        {
            var vacancy = await GetAuthorisedVacancyAsync(client, vrm, routeName);

            CheckCanEdit(vacancy);

            return vacancy;
        }

        public static async Task<Vacancy> GetAuthorisedVacancyAsync(IEmployerVacancyClient client, VacancyRouteModel vrm, string routeName)
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

            if (validRoutes.Contains(currentRouteName) || validRoutes.Last() == RouteNames.Vacancy_Preview_Get)
            {
                return;
            }

            var redirectRoute = validRoutes.Last();
            
            throw new InvalidRouteForVacancyException(string.Format(RecruitWebExceptionMessages.RouteNotValidForVacancy, currentRouteName, redirectRoute),
                redirectRoute, new VacancyRouteModel{EmployerAccountId = vacancy.EmployerAccountId, VacancyId = vacancy.Id});
        }

        public static IList<string> GetValidRoutesForVacancy(Vacancy vacancy)
        {
            var validRoutes = new List<string>();

            validRoutes.AddRange(new [] {RouteNames.Title_Post, RouteNames.Title_Get});
            if (string.IsNullOrWhiteSpace(vacancy.Title))
                return validRoutes;

            validRoutes.AddRange(new[] { RouteNames.ShortDescription_Post, RouteNames.ShortDescription_Get });
            if (string.IsNullOrWhiteSpace(vacancy.ShortDescription))
                return validRoutes;

            validRoutes.AddRange(new[] { RouteNames.Employer_Post, RouteNames.Employer_Get});
            if (string.IsNullOrWhiteSpace(vacancy.EmployerLocation?.Postcode))
                return validRoutes;
            
            validRoutes.AddRange(new[] { RouteNames.Training_Post, RouteNames.Training_Get});
            if (string.IsNullOrWhiteSpace(vacancy.ProgrammeId))
                return validRoutes;

            validRoutes.AddRange(new[] { RouteNames.Wage_Post, RouteNames.Wage_Get});
            if (vacancy.Wage?.WageType == null)
                return validRoutes;
            
            validRoutes.AddRange(new []{RouteNames.SearchResultPreview_Post, RouteNames.SearchResultPreview_Get});
            if (vacancy.HasCompletedPart1 == false)
                return validRoutes;

            validRoutes.Add(RouteNames.Vacancy_Preview_Get);
            return validRoutes;
        }

        public static string GetNextPart1WizardRoute(string currentRouteName)
        {
            switch (currentRouteName)
            {
                case RouteNames.Title_Post:
                    return RouteNames.ShortDescription_Get;
                case RouteNames.ShortDescription_Post:
                    return RouteNames.Employer_Get;
                case RouteNames.Employer_Post:
                    return RouteNames.Training_Get;
                case RouteNames.Training_Post:
                    return RouteNames.Wage_Get;
                case RouteNames.Wage_Post:
                    return RouteNames.SearchResultPreview_Get;
                default:
                    throw new NotImplementedException($"No next route configured for '{currentRouteName}'");
            }
        }

        public static VacancyRouteParameters GetRedirectRouteParametersForVacancy(Vacancy vacancy, string vacancyPreviewFragment, string currentRouteName)
        {
            string routeName;
            if (vacancy.HasCompletedPart1 == false)
            {
                //We are in wizard mode (Steps 1-6) so get the next step
                routeName = GetNextPart1WizardRoute(currentRouteName);
            }
            else
            {
                //otherwise redirect to the last valid route
                var validRoutes = GetValidRoutesForVacancy(vacancy);
                routeName = validRoutes.Last();
            }

            var fragment = routeName == RouteNames.Vacancy_Preview_Get ? vacancyPreviewFragment : null;
            return new VacancyRouteParameters(routeName, vacancy.Id, fragment);
        }

        public static VacancyRouteParameters GetCancelButtonRouteParametersForVacancy(Vacancy vacancy, string vacancyPreviewFragment)
        {
            var routeName = vacancy.HasCompletedPart1
                ? RouteNames.Vacancy_Preview_Get
                : RouteNames.Dashboard_Index_Get;

            return new VacancyRouteParameters(routeName, vacancy.Id, routeName == RouteNames.Vacancy_Preview_Get ? vacancyPreviewFragment : null);
        }

        public static async Task<ApplicationReview> GetAuthorisedApplicationReviewAsync(IEmployerVacancyClient client, ApplicationReviewRouteModel rm)
        {
            var applicationReview = await client.GetApplicationReviewAsync(rm.ApplicationReviewId);

            if (applicationReview.EmployerAccountId == rm.EmployerAccountId)
            {
                return applicationReview;
            }

            throw new AuthorisationException(string.Format(ExceptionMessages.ApplicationReviewUnauthorisedAccess, rm.EmployerAccountId, applicationReview.EmployerAccountId, applicationReview.Id, applicationReview.VacancyReference));
        }
    }
}
