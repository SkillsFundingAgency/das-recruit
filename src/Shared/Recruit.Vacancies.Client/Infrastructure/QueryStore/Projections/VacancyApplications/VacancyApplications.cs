using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications
{
    public class VacancyApplications : QueryProjectionBase
    {
        public long VacancyReference { get; set; }
        public List<VacancyApplication> Applications { get; set; }
    }
}
