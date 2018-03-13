using SFA.DAS.EAS.Account.Api.Types;
using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Domain.Projections
{
    public class EmployerVacancyData : QueryProjectionBase
    {
        public IEnumerable<LegalEntityViewModel> LegalEntities { get; set; }
    }
}