using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo
{
    public class ProviderEditVacancyInfo : QueryProjectionBase
    {
        public ProviderEditVacancyInfo() : base(QueryViewType.EditVacancyInfo.TypeName)
        {}

        public IEnumerable<EmployerInfo> Employers { get; set; }
    }
}