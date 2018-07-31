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
        public int SubmissionCount { get; set; }
        public DateTime? SlaDeadline { get; set; }

        /// <summary>
        /// We can only approve reviews that are under review.
        /// </summary>
        public bool CanApprove => Status == ReviewStatus.UnderReview;

        /// <summary>
        /// We can only refer when the review is in progress.
        /// </summary>
        public bool CanRefer => Status == ReviewStatus.UnderReview;

        /// <summary>
        /// We can only start the review when it is pending.
        /// </summary>
        public bool CanStart => Status == ReviewStatus.PendingReview;
    }
}