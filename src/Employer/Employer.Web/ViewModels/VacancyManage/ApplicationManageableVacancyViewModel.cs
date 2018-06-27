using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;

namespace Esfa.Recruit.Employer.Web.ViewModels
{
    public class ApplicationManageableVacancyViewModel : DisplayVacancyViewModel
    {
        public List<VacancyApplication> Applications { get; internal set; }
    }
}
