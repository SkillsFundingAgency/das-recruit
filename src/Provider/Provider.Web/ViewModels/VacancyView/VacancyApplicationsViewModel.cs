using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;

namespace Esfa.Recruit.Provider.Web.ViewModels.VacancyView
{
    public class VacancyApplicationsViewModel : VacancyRouteModel
    {
        public List<VacancyApplication> Applications { get; internal set; }

        public IList<IGrouping<ApplicationReviewStatus, VacancyApplication>> OrderedApplications => Applications.OrderByDescending(app => app.SubmittedDate)
            .GroupBy(app => app.Status)
            .OrderBy(g => g.Key)
            .ToList();

        public bool ShowDisability { get; internal set; }
    }
}
