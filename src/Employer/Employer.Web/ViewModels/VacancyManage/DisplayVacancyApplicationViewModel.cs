using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;
using System.Collections.Generic;
using System.Linq;

namespace Esfa.Recruit.Employer.Web.ViewModels
{
    public abstract class DisplayVacancyApplicationViewModel : DisplayVacancyViewModel
    {
        public IList<VacancyApplication> Applications { get; internal set; }
        public IList<IGrouping<ApplicationReviewStatus, VacancyApplication>> OrderedApplications => Applications.OrderByDescending(app => app.SubmittedDate)
                                                                                                                .GroupBy(app => app.Status)
                                                                                                                .OrderBy(g => g.Key)
                                                                                                                .ToList();

        public bool HasApplications => Applications.Any();
    }
}