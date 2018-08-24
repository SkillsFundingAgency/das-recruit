using System;

namespace Esfa.Recruit.Qa.Web.ViewModels
{
    public struct VacancyReviewSearchModel
    {
        public Guid ReviewId { get; set; }

        public string EmployerName { get; set; }

        public string VacancyTitle { get; set; }

        public string VacancyReference { get; set; }

        public DateTime SubmittedDate { get; set; }

        public DateTime ClosingDate { get; set; }

        public string AssignedTo { get; set; }

        public string AssignedTimeElapsed { get; set; }

        public bool IsAvailableForReview => string.IsNullOrEmpty(AssignedTo);

        public string AssignmentInfo => 
            IsAvailableForReview 
            ? null 
            : $"Assigned to {AssignedTo}. Being reviewed for {AssignedTimeElapsed}.";
    }
}
