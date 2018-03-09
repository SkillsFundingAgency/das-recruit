using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Domain.Projections
{
    public class Dashboard : QueryProjectionBase
    {
        public IEnumerable<VacancySummary> Vacancies { get; set; }
    }
}
