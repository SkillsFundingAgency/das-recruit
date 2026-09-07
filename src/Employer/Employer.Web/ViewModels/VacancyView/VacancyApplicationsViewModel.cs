#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;

namespace Esfa.Recruit.Employer.Web.ViewModels.VacancyManage
{
    public class VacancyApplicationsViewModel : VacancyRouteModel
    {
        public IEnumerable<VacancyApplication> Applications { get; internal set; }

        public AvailableWhere? AvailableWhere { get; set; }
        public PagerViewModel Pager { get; internal set; }
        public List<string> EmploymentLocations { get; set; } = [];
        public string? SelectedLocation { get; set; }
        public string? SelectedApplicantName { get; set; }
        public int TotalUnfilteredApplicationsCount { get; set; } = 0;
        public int TotalFilteredApplicationsCount { get; set; } = 0;
        public bool ShowLocationNoResultsLabel =>
            TotalFilteredApplicationsCount == 0
            && IsActiveFilter(SelectedLocation)
            && !IsActiveFilter(SelectedApplicantName);

        public bool ShowCombinedOrApplicantNoResultsLabel =>
            TotalFilteredApplicationsCount == 0
            && IsActiveFilter(SelectedApplicantName);

        public string NoResultsLabelText
        {
            get
            {
                var hasApplicant = IsActiveFilter(SelectedApplicantName);
                var hasLocation = IsActiveFilter(SelectedLocation);

                return (hasApplicant, hasLocation) switch
                {
                    (true, true) => $"0 results found for '{SelectedApplicantName}' in '{SelectedLocation}'",
                    (true, false) => $"0 results for '{SelectedApplicantName}'",
                    (false, true) => $"0 results for '{SelectedLocation}'",
                    _ => string.Empty
                };
            }
        }

        public string NoResultsHintText
        {
            get
            {
                var hasApplicant = IsActiveFilter(SelectedApplicantName);
                var hasLocation = IsActiveFilter(SelectedLocation);

                return (hasApplicant, hasLocation) switch
                {
                    (true, true) => "Check your spelling or remove the location filter.",
                    (true, false) => "Check your spelling or search for a different name.",
                    (false, true) => "No applications for this location have been received.",
                    _ => string.Empty
                };
            }
        }

        private static bool IsActiveFilter(string filter) =>
            !string.IsNullOrEmpty(filter)
            && !filter.Equals("All", StringComparison.OrdinalIgnoreCase);

        public bool HasApplications => Applications != null && Applications.Any();
        public bool HasNoApplications => !HasApplications;

        public UserType UserType { get; internal set; }
        public bool ShowDisability { get; internal set; }
        public bool VacancySharedByProvider { get; internal set; }
        public bool CanShowMultipleApplicationsUnsuccessfulLink =>
            (Applications?.Any(app => app.Status != ApplicationReviewStatus.Successful
                                      && app.Status != ApplicationReviewStatus.Unsuccessful) ?? false)
            && TotalUnfilteredApplicationsCount > 1;
        public bool CanShowCandidateAppliedLocations => Applications?.Any(app => app.CanShowCandidateAppliedLocations) ?? false;
        public bool CanShowLocationFilter => TotalUnfilteredApplicationsCount > 0 && AvailableWhere is Esfa.Recruit.Vacancies.Client.Domain.Entities.AvailableWhere
            .MultipleLocations;
    }
}