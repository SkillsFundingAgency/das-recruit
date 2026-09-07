using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo
{
    public class ProviderEditVacancyInfo
    {
        public IEnumerable<EmployerInfo> Employers { get; set; }
        public bool HasProviderAgreement { get; set; }
    }
}