using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo
{
    public class EmployerInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<LegalEntity> LegalEntities { get; set; }
    }
}