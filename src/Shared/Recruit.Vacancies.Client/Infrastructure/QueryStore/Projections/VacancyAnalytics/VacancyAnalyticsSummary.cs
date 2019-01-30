namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyAnalytics
{
    public class VacancyAnalyticsSummary : QueryProjectionBase
    {
        public VacancyAnalyticsSummary() : base(QueryViewType.VacancyAnalyticsSummary.TypeName)
        {
        }

        public long VacancyReference { get; set; }

        public int NoOfApprenticeshipSearches { get; set; }
        public int NoOfApprenticeshipSavedSearchAlerts { get; set; }
        public int NoOfApprenticeshipSaved { get; set; }
        public int NoOfApprenticeshipDetailsViews { get; set; }
        public int NoOfApprenticeshipApplicationsCreated { get; set; }
        public int NoOfApprenticeshipApplicationsSubmitted { get; set; }
    }
}