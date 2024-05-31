using System.Collections.Generic;

namespace SFA.DAS.Recruit.Api.Models
{
    public class GetClosedVacanciesByReferenceRequest
    {
        public List<long> VacancyReferences { get; set; }
    }
}
