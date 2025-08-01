using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;

namespace Esfa.Recruit.Provider.Web.ViewModels.VacancyView
{
    public class VacancyApplicationsViewModel : VacancyRouteModel
    {
        public List<VacancyApplication> Applications { get; set; }

        public PagerViewModel Pager { get; internal set; }

        public List<string> EmploymentLocations { get; set; } = [];
        public string? SelectedLocation { get; set; }
        public int TotalUnfilteredApplicationsCount { get; set; } = 0;
        public int TotalFilteredApplicationsCount { get; set; } = 0;

        public string FilteredApplicationsLabelText => TotalFilteredApplicationsCount == 1
            ? "1 Application"
            : $"{TotalFilteredApplicationsCount} Applications";

        public bool HasApplications => Applications is not null && Applications.Any();
        public bool HasNoApplications => !HasApplications;

        public bool ShowDisability { get; internal set; }

        public bool CanShowShareMultipleApplicationsLink =>
            Applications?.Any(app => app.Status == ApplicationReviewStatus.New || app.Status == ApplicationReviewStatus.InReview) ?? false;

        public bool CanShowMultipleApplicationsUnsuccessfulLink =>
          Applications?.Any(app => app.Status != ApplicationReviewStatus.Successful && app.Status != ApplicationReviewStatus.Unsuccessful) ?? false;

        public bool CanShowCandidateAppliedLocations => Applications?.Any(app => app.CanShowCandidateAppliedLocations) ?? false;
    }
}
