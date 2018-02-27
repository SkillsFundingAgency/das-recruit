using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Projections;
using System.Collections.Generic;

namespace Esfa.Recruit.Employer.Web.ViewModels
{
    public class DashboardViewModel
    {
        public string EmployerName { get; set; }

        public IList<VacancySummary> Vacancies { get; set; }
    }
}
