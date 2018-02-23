using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Employer.Web.ViewModels
{
    public class DashboardViewModel
    {
        public string EmployerName { get; set; }

        public IList<Vacancy> Vacancies { get; set; }
    }
}
