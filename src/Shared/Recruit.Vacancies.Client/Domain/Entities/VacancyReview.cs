using System;
using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Domain.Entities
{
    public class VacancyReview
    {
        public const int AssignationTimeoutHours = 3;

        public Guid Id { get; set; }
        public long VacancyReference { get; set; }
        public string Title { get; set; }
        public ReviewStatus Status { get; set; }

        /// <summary>
        /// Timestamp when this review record was created
        /// </summary>
        public DateTime? CreatedDate { get; set; }

        /// <summary>
        /// Date when review was assigned 
        /// </summary>
        public DateTime? ReviewedDate { get; set; }

        public VacancyUser ReviewedByUser { get; set; }
        
        /// <summary>
        /// Date when the review status was set to closed
        /// </summary>
        public DateTime? ClosedDate { get; set; }
        public ManualQaOutcome? ManualOutcome { get; set; }
        public string ManualQaComment { get; set; }
        public List<ManualQaFieldIndicator> ManualQaFieldIndicators { get; set; }
        public string PrivateReviewNotes { get; set; }
        public string EmployerAccountId { get; set; }
        public VacancyUser SubmittedByUser { get; set; }
        public int SubmissionCount { get; set; }
        public DateTime? SlaDeadline { get; set; }

        /// <summary>
        /// Snapshot of the vacancy at the time of creating this review record
        /// </summary>
        public Vacancy VacancySnapshot { get; set; }

        /// <summary>
        /// List of field identifiers, updated by the user before resubmitting the vacancy for review 
        /// </summary>
        public List<string> UpdatedFieldIdentifiers { get; set; }


        /// <summary>
        /// We can only approve reviews that are under review.
        /// </summary>
        public bool CanApprove => Status == ReviewStatus.UnderReview;

        /// <summary>
        /// We can only refer when the review is in progress.
        /// </summary>
        public bool CanRefer => Status == ReviewStatus.UnderReview;

        /// <summary>
        /// We can only assign the review when it is pending or underreview.
        /// </summary>
        public bool CanAssign => Status == ReviewStatus.PendingReview || Status == ReviewStatus.UnderReview;

        public DateTime? AssignationExpiry => ReviewedDate?.AddHours(AssignationTimeoutHours);
    }
}