using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Provider.Web.ViewModels.VacancyAnalytics
{
    public class VacancyAnalyticsViewModel : VacancyRouteModel
    {
        public long VacancyReference { get; internal set; }
        public VacancyAnalyticsSummaryViewModel AnalyticsSummary { get; internal set; }
        public bool HasAnalytics => AnalyticsSummary != null;
        public bool IsApplyThroughFaaVacancy { get; internal set; }
        public bool IsApplyThroughExternalApplicationSiteVacancy => !IsApplyThroughFaaVacancy;
    }
}
