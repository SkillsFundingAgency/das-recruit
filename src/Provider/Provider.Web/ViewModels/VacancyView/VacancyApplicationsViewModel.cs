using System;
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

        public AvailableWhere? AvailableWhere { get; set; }
        public PagerViewModel Pager { get; internal set; }

        public List<string> EmploymentLocations { get; set; } = [];
        public string? SelectedLocation { get; set; }
        public int TotalUnfilteredApplicationsCount { get; set; } = 0;
        public int TotalFilteredApplicationsCount { get; set; } = 0;

        public string FilteredApplicationsLabelText => TotalFilteredApplicationsCount == 1
            ? $"1 result for '{SelectedLocation}'"
            : $"{TotalFilteredApplicationsCount} results for '{SelectedLocation}'";

        public bool ShowFilteredApplicationsLabelText => !string.IsNullOrEmpty(SelectedLocation) && !string.Equals(SelectedLocation, "All", StringComparison.InvariantCultureIgnoreCase);

        public bool HasApplications => Applications is not null && Applications.Count > 0;
        public bool HasNoApplications => !HasApplications;

        public bool ShowDisability { get; internal set; }

        public bool CanShowShareMultipleApplicationsLink =>
            (Applications?.Any(app => app.Status is ApplicationReviewStatus.New or ApplicationReviewStatus.InReview) ??
             false)
            && TotalUnfilteredApplicationsCount > 1;

        public bool CanShowMultipleApplicationsUnsuccessfulLink =>
            (Applications?.Any(app => app.Status != ApplicationReviewStatus.Successful
                                      && app.Status != ApplicationReviewStatus.Unsuccessful) ?? false)
            && TotalUnfilteredApplicationsCount > 1;

        public bool CanShowCandidateAppliedLocations => Applications?.Any(app => app.CanShowCandidateAppliedLocations) ?? false;

        public bool CanShowLocationFilter => (TotalUnfilteredApplicationsCount > 0) && AvailableWhere is Esfa.Recruit.Vacancies.Client.Domain.Entities.AvailableWhere
            .MultipleLocations;
    }
}
