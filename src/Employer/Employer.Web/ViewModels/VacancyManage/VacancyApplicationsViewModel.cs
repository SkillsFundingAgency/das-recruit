using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;

namespace Esfa.Recruit.Employer.Web.ViewModels.VacancyManage
{
    public class VacancyApplicationsViewModel
    {
        public List<VacancyApplication> Applications { get; set; }

        public IList<IGrouping<ApplicationReviewStatus, VacancyApplication>> OrderedApplications => Applications.OrderByDescending(app => app.SubmittedDate)
            .GroupBy(app => app.Status)
            .OrderBy(g => g.Key)
            .ToList();

        public bool ShowDisability { get; set; }
    }
}
