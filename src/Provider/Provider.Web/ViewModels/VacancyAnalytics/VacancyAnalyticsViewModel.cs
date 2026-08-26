using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Provider.Web.ViewModels.VacancyAnalytics
{
    public class VacancyAnalyticsViewModel
    {
        public VacancyAnalyticsSummaryViewModel AnalyticsSummary { get; internal set; }
        public bool HasAnalytics => AnalyticsSummary != null;
    }
}