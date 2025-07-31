using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;

namespace Esfa.Recruit.Employer.Web.ViewModels.VacancyManage
{
    public class VacancyApplicationsViewModel : VacancyRouteModel
    {
        public IEnumerable<VacancyApplication> Applications { get; internal set; }

        public AvailableWhere? AvailableWhere { get; set; }
        public List<string> EmploymentLocations { get; set; } = [];
        public string? SelectedLocation { get; set; }
        public int TotalUnfilteredApplicationsCount { get; set; } = 0;
        public string FilteredApplicationsLabelText => Applications.Count() == 1
            ? "1 Application"
            : $"{Applications.Count()} Applications";

        public bool HasApplications => Applications != null && Applications.Any();
        public bool HasNoApplications => !HasApplications;

        public UserType UserType { get; internal set; }
        public bool ShowDisability { get; internal set; }
        public bool VacancySharedByProvider { get; internal set; }
        public bool CanShowMultipleApplicationsUnsuccessfulLink =>
            Applications?.Any(app => app.Status != ApplicationReviewStatus.Successful && app.Status != ApplicationReviewStatus.Unsuccessful) ?? false;

        public bool CanShowCandidateAppliedLocations => Applications?.Any(app => app.CanShowCandidateAppliedLocations) ?? false;
        public bool CanShowLocationFilter => AvailableWhere is Esfa.Recruit.Vacancies.Client.Domain.Entities.AvailableWhere
            .MultipleLocations;
    }
}