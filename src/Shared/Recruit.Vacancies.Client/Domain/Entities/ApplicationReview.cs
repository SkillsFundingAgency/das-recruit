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
        public Application Application { get; set; }
        public DateTime? StatusUpdatedDate { get; set; }
        public VacancyUser StatusUpdatedBy { get;set; }
        public string CandidateFeedback { get; set; }
        public bool IsWithdrawn { get; set; }
        public DateTime? WithdrawnDate { get; set; }
        public bool CanReview => IsWithdrawn == false;
        public bool CanWithdraw => IsWithdrawn == false;
        public string AdditionalQuestion1 { get; set; }
        public string AdditionalQuestion2 { get; set; }
        public DateTime? DateSharedWithEmployer { get; set; }
        public DateTime? ReviewedDate { get; set; }
        public bool? HasEverBeenEmployerInterviewing { get; set; }
        public string VacancyTitle { get; set; }
    }
}
