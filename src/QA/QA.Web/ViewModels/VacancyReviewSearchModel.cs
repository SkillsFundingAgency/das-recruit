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

        public bool IsAvailableForReview { get; set; }

        public bool IsNotAvailableForReview { get; set; }

        public string AssignmentInfo { get; set; }
    }
}
