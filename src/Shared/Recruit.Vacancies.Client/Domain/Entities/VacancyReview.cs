using System;
using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Domain.Entities
{
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
        /// A review can be unassigned only if it is assigned
        /// </summary>
        public bool CanUnassign => Status == ReviewStatus.UnderReview && ReviewedByUser != null;

        public RuleSetOutcome AutomatedQaOutcome { get; set; }

        public static class FieldIdentifiers
        {
            public const string ApplicationInstructions = "ApplicationInstructions";
            public const string ApplicationMethod = "ApplicationMethod";
            public const string ApplicationUrl = "ApplicationUrl";
            public const string ClosingDate = "ClosingDate";
            public const string EmployerContact = "EmployerContact";
            public const string DisabilityConfident = "DisabilityConfident";
            public const string EmployerAddress = "EmployerAddress";
            public const string EmployerDescription = "EmployerDescription";
            public const string EmployerWebsiteUrl = "EmployerWebsiteUrl";
            public const string ExpectedDuration = "ExpectedDuration";
            public const string NumberOfPositions = "NumberOfPositions";
            public const string OutcomeDescription = "OutcomeDescription";
            public const string PossibleStartDate = "PossibleStartDate";
            public const string Provider = "Provider";
            public const string Qualifications = "Qualifications";
            public const string ShortDescription = "ShortDescription";
            public const string Skills = "Skills";
            public const string ThingsToConsider = "ThingsToConsider";
            public const string Title = "Title";
            public const string Training = "Training";
            public const string TrainingDescription = "TrainingDescription";
            public const string TrainingLevel = "TrainingLevel";
            public const string VacancyDescription = "VacancyDescription";
            public const string Wage = "Wage";
            public const string WorkingWeek = "WorkingWeek";
        }
    }
}