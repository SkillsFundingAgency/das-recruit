using System.Linq;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Application;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Humanizer;

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
        public bool IsDisabilityConfident { get; internal set; }
        public bool IsApplyThroughFaaVacancy { get; internal set; }
        public bool IsWithdrawn => !string.IsNullOrEmpty(WithdrawnDate);
        public bool IsClosedBlockedByQa { get; set; }
        public VacancyApplicationsViewModel Applications { get; internal set; }
        public bool HasApplications => TotalUnfilteredApplicationsCount > 0;
        public bool HasNoApplications => TotalUnfilteredApplicationsCount == 0;
        public bool CanShowNoApplicationsInsetText => !IsVacancyArchived && !IsVacancyRejected;
        public int TotalUnfilteredApplicationsCount => Applications?.TotalUnfilteredApplicationsCount ?? 0;
        public bool ShowEmployerApplications => !Applications.VacancySharedByProvider;
        public bool ShowSharedApplications => HasApplications && Applications.VacancySharedByProvider;
        public bool CanShowMultipleApplicationsUnsuccessfulLink => (IsVacancyLive || IsVacancyClosed || IsVacancyArchived) && Applications.CanShowMultipleApplicationsUnsuccessfulLink && ShowEmployerApplications;
        public bool CanShowApplicationsSection => Status is not (VacancyStatus.Review or VacancyStatus.Draft or VacancyStatus.Submitted);
        public bool CanShowStartAndClosingDates => !IsVacancyArchived;
        public bool CanShowEditVacancyLink { get; internal set; }
        public bool CanShowCloseVacancyLink { get; internal set; }
        public bool CanShowDeleteLink { get; internal set; }
        public bool CanShowArchiveLink { get; internal set; }
        public string VacancyClosedInfoMessage { get; internal set; }
        public string EmployerReviewedApplicationHeaderMessage { get; internal set; }
        public string EmployerReviewedApplicationBodyMessage { get; internal set; }
        public string TransferredProviderName { get; internal set; }
        public string TransferredOnDate { get; internal set; }
        public bool HasVacancyClosedInfoMessage => !string.IsNullOrEmpty(VacancyClosedInfoMessage);
        public bool CanShowApplicationReviewStatusHeader => !string.IsNullOrEmpty(EmployerReviewedApplicationHeaderMessage);
        public string ApplicationStatusChangeHeaderMessage { get; internal set; }
        public bool CanShowApplicationStatusChangeBanner => !string.IsNullOrEmpty(ApplicationStatusChangeHeaderMessage);
        public bool CanShowArchiveInsetText => IsVacancyArchived || IsVacancyClosed;
        public bool CanShowVacancyAnalytics => IsVacancyLive || IsVacancyClosed || IsVacancyArchived ;
        public bool IsVacancyLive => Status == VacancyStatus.Live;
        public bool IsVacancyClosed => Status == VacancyStatus.Closed;
        public bool IsVacancyArchived => Status == VacancyStatus.Archived;
        public bool IsVacancyRejected => Status == VacancyStatus.Rejected;
        public bool IsTransferred => !string.IsNullOrWhiteSpace(TransferredProviderName) && !string.IsNullOrWhiteSpace(TransferredOnDate);
        public bool CanClone { get; internal set; }
        public string ViewBagTitle => ShowEmployerApplications ? "Manage Advert" : $"{Title} shared applications";
        public string ApplicationReviewsUnsuccessfulBannerHeader { get; internal set; }
        public bool CanShowApplicationsUnsuccessfulBanner => !string.IsNullOrEmpty(ApplicationReviewsUnsuccessfulBannerHeader);
        public ApprenticeshipTypes ApprenticeshipType { get; internal set; }
        public FilteringOptions FilteringOptions { get; internal set; }
        public int TotalOutstandingApplicationsCount =>
            Applications.Applications.Count(x => x.Status == ApplicationReviewStatus.New && x.IsNotWithdrawn);
        public bool CanShowOutstandingApplicationsBannerMessage => (IsVacancyLive || IsVacancyClosed) && TotalOutstandingApplicationsCount > 0;
        public string TotalOutstandingApplicationsBannerMessage
        {
            get
            {
                var isPlural = TotalOutstandingApplicationsCount > 1;
                var applicantWord = isPlural ? Constants.ApplicantWord.Pluralize() : Constants.ApplicantWord;
                var verbWord = isPlural ? "are" : "is";

                return $"{TotalOutstandingApplicationsCount} {applicantWord} {verbWord} waiting for a response. " +
                       "Notify them with our standard message or edit with feedback.";
            }
        }
    }
}