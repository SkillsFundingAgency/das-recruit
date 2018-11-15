using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Jobs.Configuration
{
    public class RecruitWebJobsSystemConfiguration
    {
        public string Id { get; set; }
        public IList<string> DisabledJobs { get; set; }
    }
}