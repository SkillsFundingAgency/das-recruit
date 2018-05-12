using System;

namespace Esfa.Recruit.Vacancies.Client.Domain.Entities
{
    public class VacancyReview
    {
        public Guid Id { get; set; }
        public long VacancyReference { get; set; }
        public string Title { get; set; }
        public ReviewStatus Status { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ReviewedDate { get; set; }
        public VacancyUser ReviewedByUser { get; set; }
        public DateTime? ClosedDate { get; set; }
        public ManualQaOutcome? ManualOutcome { get; set; }
        public string PrivateReviewNotes { get; set; }
        public string EmployerAccountId { get; set; }
        public VacancyUser SubmittedByUser { get; set; }
    }
}