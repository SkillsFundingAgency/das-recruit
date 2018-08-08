using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Dashboard
{
    public class Dashboard : QueryProjectionBase
    {
        public Dashboard() : base(QueryViewType.Dashboard.TypeName)
        {
        }

        public IEnumerable<VacancySummary> Vacancies { get; set; }
    }
}
