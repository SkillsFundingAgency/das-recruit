using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications
{
    public class VacancyApplications : QueryProjectionBase
    {
        public VacancyApplications() : base(QueryViewType.VacancyApplications.TypeName)
        {
        }

        public long VacancyReference { get; set; }
        public List<VacancyApplication> Applications { get; set; }
    }
}
