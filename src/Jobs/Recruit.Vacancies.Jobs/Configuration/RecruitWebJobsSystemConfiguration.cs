using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Jobs.Configuration
{
    public class RecruitWebJobsSystemConfiguration
    {
        public string Id { get; set; }
        public IList<string> DisabledJobs { get; set; } = new List<string>();
        public int? QueryStoreDocumentsStaleByDays { get; set; }
        public int? DraftVacanciesStaleByDays { get; set; }
        public int? ReferredVacanciesStaleByDays { get; set; }
    }
}