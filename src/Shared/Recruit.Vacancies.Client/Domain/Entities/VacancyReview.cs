using System;
using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Domain.Entities;

public class VacancyReview
{
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
    /// A review can be unassigned only if it is assigned
    /// </summary>
    public bool CanUnassign => Status == ReviewStatus.UnderReview && ReviewedByUser != null;

    public bool IsPending => Status == ReviewStatus.New || Status == ReviewStatus.PendingReview;

    public RuleSetOutcome AutomatedQaOutcome { get; set; }
    public IEnumerable<RuleOutcomeIndicator> AutomatedQaOutcomeIndicators { get; set; } = new List<RuleOutcomeIndicator>();

    public IEnumerable<ManualQaFieldEditIndicator> ManualQaFieldEditIndicators { get; set; } =
        new List<ManualQaFieldEditIndicator>();
        
    public List<string> DismissedAutomatedQaOutcomeIndicators { get; set; }
        
    public DateTime? MigrationDate { get; set; }
}