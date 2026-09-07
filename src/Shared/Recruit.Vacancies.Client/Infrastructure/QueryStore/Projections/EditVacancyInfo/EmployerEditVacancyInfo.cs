using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo
{
    public class EmployerEditVacancyInfo
    {
        public IEnumerable<LegalEntity> LegalEntities { get; set; }
    }
}