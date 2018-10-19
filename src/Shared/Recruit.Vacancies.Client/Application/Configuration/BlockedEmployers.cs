using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Application.Configuration
{
    public class BlockedEmployers
    {
        public string Id { get; set; }
        public List<string> EmployerAccountIds { get; set; }
    }
}
