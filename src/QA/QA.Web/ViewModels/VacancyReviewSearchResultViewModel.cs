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

        public bool IsAssignedToLoggedInUser { get; internal set; }

        public bool IsAvailableForReview { get; internal set; }

        public bool IsClosed { get; set; }

        public bool ShowAssignmentInfoCaption => !IsAvailableForReview && !IsClosed;

        public bool CanShowReviewLink => (IsAvailableForReview || IsAssignedToLoggedInUser) && !IsClosed;

        public string ClosedReviewAssignmentCaption => $"Reviewed by {AssignedToCaption}";

        internal string AssignedTo { get; set; }

        internal string AssignedTimeElapsed { get; set; }

        private string AssignedToCaption => IsAssignedToLoggedInUser ? "you" : AssignedTo;
            
        private string AssignedTimeElapsedCaption => string.IsNullOrEmpty(AssignedTimeElapsed) 
                                                    ? "now" 
                                                    : $"for {AssignedTimeElapsed}";

        public string GetAssignmentInfoCaption()
        {
            return $"Assigned to {AssignedToCaption}. Being reviewed {AssignedTimeElapsedCaption}.";
        }

        public bool ShowReviewer => IsClosed && !IsVacancyDeleted;
        public bool IsVacancyDeleted { get; set; }
    }
}