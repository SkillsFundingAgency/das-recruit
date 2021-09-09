using System.Collections.Generic;

namespace SFA.DAS.Recruit.Api.Models
{
    public class EmployerAccountSummary
    {
        public string EmployerAccountId { get; set; }
        public int TotalNoOfVacancies { get; set; }
        public IDictionary<string, int> TotalVacancyStatusCounts { get; set; }
        public IEnumerable<LegalEntityVacancySummary> LegalEntityVacancySummaries { get; set; }
    }

    public class LegalEntityVacancySummary
    {
        public long? LegalEntityId { get; set; }
        public string LegalEntityName { get; set; }
        public IDictionary<string, int> VacancyStatusCounts { get; set; }
    }
}