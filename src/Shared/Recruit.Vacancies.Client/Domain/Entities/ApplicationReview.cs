using System;

namespace Esfa.Recruit.Vacancies.Client.Domain.Entities
{
    public class ApplicationReview
    {
        public Guid Id { get; set; }
        public Guid CandidateId { get; set; }
        public long VacancyReference { get; set; }
        public ApplicationReviewStatus Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime SubmittedDate { get; set; }
        public string EmployerAccountId { get; set; }
        public Application Application { get; set; }

    }
}
