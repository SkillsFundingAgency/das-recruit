using System;

namespace Esfa.Recruit.Qa.Web.ViewModels
{
    public class VacancyReviewSearchResultViewModel
    {
        public Guid ReviewId { get; internal set; }

        public string EmployerName { get; internal set; }

        public string VacancyTitle { get; internal set; }

        public string VacancyReference { get; internal set; }

        public DateTime SubmittedDate { get; internal set; }

        public DateTime ClosingDate { get; internal set; }

        public bool IsAvailableForReview { get; internal set; }

        public bool IsNotAvailableForReview => !IsAvailableForReview;

        internal string AssignedTo { get; set; }

        internal string AssignedTimeElapsed { get; set; }

        private string AssignedToCaption => 
            IsNotAvailableForReview 
            ? AssignedTo ?? "you"
            : null;
            
        private string AssignedTimeElapsedCaption => string.IsNullOrEmpty(AssignedTimeElapsed) 
                                                    ? "now" 
                                                    : $"for {AssignedTimeElapsed}";

        public string GetAssignmentInfoCaption()
        {
            return $"Assigned to {AssignedToCaption}. Being reviewed {AssignedTimeElapsedCaption}.";
        }
    }
}