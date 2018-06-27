using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;
using System.Collections.Generic;
using System.Linq;

namespace Esfa.Recruit.Employer.Web.ViewModels
{
    public abstract class DisplayVacancyApplicationViewModel : DisplayVacancyViewModel
    {
        public List<VacancyApplication> Applications { get; internal set; }

        public bool HasApplications => Applications.Any();
    }
}