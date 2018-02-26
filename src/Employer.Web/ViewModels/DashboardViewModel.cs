using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using System.Collections.Generic;

namespace Esfa.Recruit.Employer.Web.ViewModels
{
    public class DashboardViewModel
    {
        public string EmployerName { get; set; }

        public IList<Vacancy> Vacancies { get; set; }
    }
}
