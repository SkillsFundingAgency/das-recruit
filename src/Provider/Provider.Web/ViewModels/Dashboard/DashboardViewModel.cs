using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Provider;
using Humanizer;

namespace Esfa.Recruit.Provider.Web.ViewModels.Dashboard
{
    public class DashboardViewModel
    {
        public ProviderDashboardSummary ProviderDashboardSummary { get; internal set; }
        public AlertsViewModel Alerts { get; internal set; }

        public bool HasOneVacancy => ProviderDashboardSummary.HasOneVacancy;
        //public Guid CurrentVacancyId => HasOneVacancy ? Vacancies.Single().Id : new Guid();
        public int VacancyCountDraft => ProviderDashboardSummary.Draft;
        public string VacancyTextDraft => "vacancy".ToQuantity(VacancyCountDraft, ShowQuantityAs.None);
        public bool HasDraftVacancy => VacancyCountDraft > 0;
        public int VacancyCountLive => ProviderDashboardSummary.Live;
        public string VacancyTextLive => "vacancy".ToQuantity(VacancyCountLive, ShowQuantityAs.None);
        public bool HasLiveVacancy => VacancyCountLive > 0;
        public int VacancyCountClosed => ProviderDashboardSummary.Closed;
        public string VacancyTextClosed => "vacancy".ToQuantity(VacancyCountClosed, ShowQuantityAs.None);
        public bool HasClosedVacancy => VacancyCountClosed > 0;
        public int VacancyCountReferred => ProviderDashboardSummary.Referred;
        public string VacancyTextReferred => "vacancy".ToQuantity(VacancyCountReferred, ShowQuantityAs.None);
        public bool HasReferredVacancy => VacancyCountReferred > 0;
        public int VacancyCountReview => ProviderDashboardSummary.Review;
        public bool HasReviewVacancy => VacancyCountReview > 0;
        public int VacancyCountSubmitted => ProviderDashboardSummary.Submitted;
        public bool HasSubmittedVacancy => VacancyCountSubmitted > 0;
        public int NoOfNewApplications => ProviderDashboardSummary.NumberOfNewApplications;
        public bool HasNewApplications => NoOfNewApplications > 0;
        public bool ShowAllApplications => ProviderDashboardSummary.HasApplications;
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