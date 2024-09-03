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
        public UserType UserType { get; internal set; }
        public bool ShowDisability { get; internal set; }
        public bool VacancySharedByProvier { get; internal set; }
        public bool CanShowMultipleApplicationsUnsuccessfulLink =>
            Applications?.Any(app => app.Status != ApplicationReviewStatus.Successful && app.Status != ApplicationReviewStatus.Unsuccessful) ?? false;
    }
}
