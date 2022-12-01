using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Employer
{
    public class EmployerDashboard : QueryProjectionBase
    {
        public EmployerDashboard() : base(QueryViewType.EmployerDashboard.TypeName)
        {
        }

        public IEnumerable<VacancySummary> Vacancies { get; set; }

    }
}
