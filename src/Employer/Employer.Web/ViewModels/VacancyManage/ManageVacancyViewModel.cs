using System.Linq;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Employer.Web.ViewModels.VacancyManage
{
    public class ManageVacancyViewModel : VacancyRouteModel
    {
        public string Title { get; internal set; }
        public VacancyStatus Status { get; internal set; }
        public string VacancyReference { get; internal set; }
        public string ClosingDate { get; internal set; }
        public string WithdrawnDate { get; internal set;}
        public string PossibleStartDate { get; internal set; }
        public string AnalyticsAvailableAfterApprovalDate { get; internal set; }
        public bool IsDisabilityConfident { get; internal set; }
        public bool IsApplyThroughFaaVacancy { get; internal set; }
        public bool IsApplyThroughExternalApplicationSiteVacancy => !IsApplyThroughFaaVacancy;
        public bool IsWithdrawn => string.IsNullOrEmpty(WithdrawnDate) == false;
        public bool IsClosedBlockedByQa { get; set; }
        public VacancyApplicationsViewModel Applications { get; internal set; }
        public bool HasApplications => Applications.Applications.Any();
        public bool HasNoApplications => Applications.Applications == null || Applications.Applications?.Any() == false;
        public bool ShowEmployerApplications => HasApplications && !Applications.VacancySharedByProvier;
        public bool ShowSharedApplications => HasApplications && Applications.VacancySharedByProvier;
        public bool CanShowMultipleApplicationsUnsuccessfulLink => (IsVacancyLive || IsVacancyClosed) && Applications.CanShowMultipleApplicationsUnsuccessfulLink && ShowEmployerApplications;

        public bool CanShowEditVacancyLink { get; internal set; }
        public bool CanShowCloseVacancyLink { get; internal set; }
        public bool CanShowDeleteLink { get; internal set; }
        public string VacancyClosedInfoMessage { get; internal set; }
        public string EmployerReviewedApplicationHeaderMessage { get; internal set; }
        public string EmployerReviewedApplicationBodyMessage { get; internal set; }
        public string TransferredProviderName { get; internal set; }
        public string TransferredOnDate { get; internal set; }
        public bool HasVacancyClosedInfoMessage => !string.IsNullOrEmpty(VacancyClosedInfoMessage);
        public bool CanShowApplicationReviewStatusHeader => !string.IsNullOrEmpty(EmployerReviewedApplicationHeaderMessage);

        public VacancyAnalyticsSummaryViewModel AnalyticsSummary { get; internal set; }

        public bool CanShowVacancyAnalytics => IsVacancyLive || IsVacancyClosed;
        public bool HasAnalytics => AnalyticsSummary != null;
        public bool IsVacancyLive => Status == VacancyStatus.Live;
        public bool IsVacancyClosed => Status == VacancyStatus.Closed;
        public bool IsTransferred => string.IsNullOrWhiteSpace(TransferredProviderName) == false && string.IsNullOrWhiteSpace(TransferredOnDate) == false;
        public bool CanClone { get; internal set; }
        public string ViewBagTitle => ShowEmployerApplications ? "Manage Advert" : "Shared applications";
    }
}