using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.VacancyView;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;

namespace Esfa.Recruit.Provider.Web.ViewModels.VacancyManage
{
    public class ManageVacancyViewModel : VacancyRouteModel
    {
        public string Title { get; internal set; }
        public VacancyStatus Status { get; internal set; }
        public string VacancyReference { get; internal set; }
        public string ClosingDate { get; internal set; }
        public string PossibleStartDate { get; internal set; }
        public string AnalyticsAvailableAfterApprovalDate { get; internal set; }
        public bool IsDisabilityConfident { get; internal set; }
        public bool IsApplyThroughFaaVacancy { get; internal set; }
        public bool IsApplyThroughExternalApplicationSiteVacancy => !IsApplyThroughFaaVacancy;

        public VacancyApplicationsViewModel Applications { get; internal set; }
        public bool HasApplications => Applications.Applications.Any();
        public bool HasNoApplications => Applications.Applications == null || Applications.Applications?.Any() == false;
        public bool CanShowEditVacancyLink { get; internal set; }
        public bool CanShowCloseVacancyLink { get; internal set; }
        public bool CanShowCloneVacancyLink { get; internal set; }
        public bool CanShowDeleteVacancyLink { get; internal set; }
        public string VacancyClosedInfoMessage { get; internal set; }
        public bool HasVacancyClosedInfoMessage => !string.IsNullOrEmpty(VacancyClosedInfoMessage);
        public string ApplicationReviewStatusHeaderInfoMessage { get; internal set; }
        public bool CanShowApplicationReviewStatusHeader => !string.IsNullOrEmpty(ApplicationReviewStatusHeaderInfoMessage);

        public VacancyAnalyticsSummaryViewModel AnalyticsSummary { get; internal set; }

        public bool CanShowVacancyAnalytics => IsVacancyLive || IsVacancyClosed;
        public bool HasAnalytics => AnalyticsSummary != null;
        public bool IsVacancyLive => Status == VacancyStatus.Live;
        public bool IsVacancyClosed => Status == VacancyStatus.Closed;
        public string WithdrawnDate { get; internal set; }
        public bool IsWithdrawn => !string.IsNullOrEmpty(WithdrawnDate);
        public bool IsApplyThroughFatVacancy { get; internal set; }
        public string SharedApplicationsBannerHeader { get; internal set; }
        public string SharedApplicationsBannerMessage { get; internal set; }
        public bool CanShowApplicationsSharedBanner => !string.IsNullOrEmpty(SharedApplicationsBannerHeader);
    }
}