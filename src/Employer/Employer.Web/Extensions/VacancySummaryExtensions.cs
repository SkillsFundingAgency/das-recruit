using System.Data;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Employer;

namespace Esfa.Recruit.Employer.Web.Extensions
{
    public static class VacancySummaryExtensions
    {
        public static string GetLinkText(this VacancySummary vacancySummary)
        {
            switch (vacancySummary.Status)
            {
                case VacancyStatus.Referred:
                    return "Edit and resubmit";
                case VacancyStatus.Draft:
                    return "Edit and submit";
            }

            return "View";
        }

        public static string GetLinkRoute(this VacancySummary vacancySummary)
        {
            if (vacancySummary.Status == VacancyStatus.Live || vacancySummary.Status == VacancyStatus.Closed)
                return RouteNames.VacancyManage_Get;

            return RouteNames.DisplayVacancy_Get;
        }
    }
}
