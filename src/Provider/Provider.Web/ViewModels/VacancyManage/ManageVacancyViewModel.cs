using System.Linq;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.VacancyView;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Provider.Web.ViewModels.VacancyManage
{
    public class ManageVacancyViewModel : VacancyRouteModel
    {
        public string Title { get; internal set; }
        public VacancyStatus Status { get; internal set; }
        public string VacancyReference { get; internal set; }
        public string EmployerName { get; internal set; }
        public string ClosingDate { get; internal set; }
        public string PossibleStartDate { get; internal set; }
        public bool IsDisabilityConfident { get; internal set; }
        public bool IsApplyThroughFaaVacancy { get; internal set; }

        public VacancyApplicationsViewModel Applications { get; internal set; }
        public bool HasApplications => TotalUnfilteredApplicationsCount > 0;
        public bool HasNoApplications => TotalUnfilteredApplicationsCount == 0;
        public int TotalUnfilteredApplicationsCount => Applications?.TotalUnfilteredApplicationsCount ?? 0;
        public bool CanShowEditVacancyLink { get; internal set; }
        public bool CanShowCloseVacancyLink { get; internal set; }
        public bool CanShowCloneVacancyLink { get; internal set; }
        public bool CanShowDeleteVacancyLink { get; internal set; }
        public string VacancyClosedInfoMessage { get; internal set; }
        public bool HasVacancyClosedInfoMessage => !string.IsNullOrEmpty(VacancyClosedInfoMessage);
        public string ApplicationReviewStatusHeaderInfoMessage { get; internal set; }
        public bool CanShowApplicationReviewStatusHeader => !string.IsNullOrEmpty(ApplicationReviewStatusHeaderInfoMessage);

        public bool CanShowVacancyAnalytics => IsVacancyLive || IsVacancyClosed;
        public bool CanShowShareMultipleApplicationsLink => (IsVacancyLive || IsVacancyClosed) && Applications.CanShowShareMultipleApplicationsLink;
        public bool CanShowMultipleApplicationsUnsuccessfulLink => (IsVacancyLive || IsVacancyClosed) && Applications.CanShowMultipleApplicationsUnsuccessfulLink;
        public bool IsVacancyLive => Status == VacancyStatus.Live;
        public bool IsVacancyClosed => Status == VacancyStatus.Closed;
        public string WithdrawnDate { get; internal set; }
        public bool IsWithdrawn => !string.IsNullOrEmpty(WithdrawnDate);
        public bool IsApplyThroughFatVacancy { get; internal set; }
        public string ApplicationReviewStatusChangeBannerHeader { get; internal set; }
        public string ApplicationReviewStatusChangeBannerMessage { get; internal set; }
        public bool CanShowApplicationsStatusChangeBanner => !string.IsNullOrEmpty(ApplicationReviewStatusChangeBannerHeader);
        public ApprenticeshipTypes ApprenticeshipType { get; internal set; }
    }
}