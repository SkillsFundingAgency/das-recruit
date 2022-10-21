using System;

namespace Esfa.Recruit.Vacancies.Client.Domain.Entities
{
    public class VacancyReviewSummary
    {
        public long VacancyReference { get; set; }
        public int SubmissionCount { get; set; }
        public DateTime? SlaDeadline { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
