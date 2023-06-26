using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Provider;
using Humanizer;

namespace Esfa.Recruit.Provider.Web.ViewModels.Dashboard
{
    public class DashboardViewModel
    {
        public ProviderDashboardSummary ProviderDashboardSummary { get; internal set; }
        public AlertsViewModel Alerts { get; internal set; }
        public int VacancyCountDraft => ProviderDashboardSummary.Draft;
        public string VacancyTextDraft => "vacancy".ToQuantity(VacancyCountDraft, ShowQuantityAs.None);
        public int VacancyCountLive => ProviderDashboardSummary.Live;
        public string VacancyTextLive => "vacancy".ToQuantity(VacancyCountLive, ShowQuantityAs.None);
        public int VacancyCountClosed => ProviderDashboardSummary.Closed;
        public string VacancyTextClosed => "vacancy".ToQuantity(VacancyCountClosed, ShowQuantityAs.None);
        public int VacancyCountReferred => ProviderDashboardSummary.Referred;
        public string VacancyTextReferred => "vacancy".ToQuantity(VacancyCountReferred, ShowQuantityAs.None);
        public int VacancyCountReview => ProviderDashboardSummary.Review;
        public int VacancyCountSubmitted => ProviderDashboardSummary.Submitted;
        public int NoOfNewApplications => ProviderDashboardSummary.NumberOfNewApplications;
        public int NumberOfReviewedApplications => ProviderDashboardSummary.NumberOfReviewedApplications;
        public bool ShowAllApplications => ProviderDashboardSummary.HasApplications;
        public string ApplicationTextReviewedApplication => "Employer reviewed applications".ToQuantity(NumberOfReviewedApplications, ShowQuantityAs.None);
        public string ApplicationTextLive => "application".ToQuantity(NoOfNewApplications, ShowQuantityAs.None);
        public int NoOfVacanciesClosingSoon => ProviderDashboardSummary.NumberClosingSoon;
        public string VacancyTextClosingSoon => "vacancy".ToQuantity(NoOfVacanciesClosingSoon, ShowQuantityAs.None);
        public int NoOfVacanciesClosingSoonWithNoApplications => ProviderDashboardSummary.NumberClosingSoonWithNoApplications;
        public string VacancyTextClosingSoonWithNoApplications => "vacancy".ToQuantity(NoOfVacanciesClosingSoonWithNoApplications, ShowQuantityAs.None);
        public bool ShowNoOfVacanciesClosingSoon => NoOfVacanciesClosingSoon > 0;
        public bool ShowNoOfVacanciesClosingSoonWithNoApplications => NoOfVacanciesClosingSoonWithNoApplications > 0;
        public bool HasAnyVacancies => ProviderDashboardSummary.HasVacancies;
        public int NumberOfVacancies => ProviderDashboardSummary.NumberOfVacancies;
        public bool HasEmployerReviewPermission { get; set; }
        public long Ukprn { get; set; }
    }
}