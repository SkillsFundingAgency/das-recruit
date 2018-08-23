using System.Collections.Generic;

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
