using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Domain.Projections
{
    public class Dashboard
    {
        public string Id { get; set; }        
        public IEnumerable<VacancySummary> Vacancies { get; set; }
    }
}
