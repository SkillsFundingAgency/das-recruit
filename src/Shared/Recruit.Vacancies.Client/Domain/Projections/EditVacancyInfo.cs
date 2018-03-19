using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Domain.Projections
{
    public class EditVacancyInfo : QueryProjectionBase
    {
        public IEnumerable<LegalEntity> LegalEntities { get; set; }
    }
}