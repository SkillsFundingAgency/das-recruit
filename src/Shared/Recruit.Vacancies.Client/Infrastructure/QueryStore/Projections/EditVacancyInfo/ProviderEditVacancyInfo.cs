using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo
{
    public class ProviderEditVacancyInfo : QueryProjectionBase
    {
        public ProviderEditVacancyInfo() : base(QueryViewType.EditVacancyInfo.TypeName)
        {}

        public IEnumerable<EmployerInfo> Employers { get; set; }
    }
}