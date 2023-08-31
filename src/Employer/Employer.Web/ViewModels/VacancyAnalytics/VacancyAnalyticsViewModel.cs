using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Employer.Web.ViewModels.VacancyAnalytics
{
    public class VacancyAnalyticsViewModel : VacancyRouteModel
    {
        public long VacancyReference { get; internal set; }
        public VacancyAnalyticsSummaryViewModel AnalyticsSummary { get; internal set; }
        public string ViewBagTitle => "Vacancy Analytics";
        public bool HasAnalytics => AnalyticsSummary != null;
        public string AnalyticsAvailableAfterApprovalDate { get; internal set; }
        public bool IsApplyThroughFaaVacancy { get; internal set; }
        public bool IsApplyThroughExternalApplicationSiteVacancy => !IsApplyThroughFaaVacancy;
    }
}
