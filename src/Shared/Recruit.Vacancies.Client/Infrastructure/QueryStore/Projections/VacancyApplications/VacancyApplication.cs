using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications
{
    public class VacancyApplication
    {
        private string _candidateName;

        public DateTime SubmittedDate { get; set; }
        public ApplicationReviewStatus Status { get; set; }
        public Guid CandidateId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CandidateName
        {
            get
            {
                if (string.IsNullOrEmpty(FirstName))
                {
                    return _candidateName;
                }
                else
                {
                    return $"{FirstName} {LastName}";
                }
            }
            set { _candidateName = value; }
        }

        public DateTime? DateOfBirth { get; set; }
        public Guid ApplicationReviewId { get; set; }
        public ApplicationReviewDisabilityStatus DisabilityStatus { get; set; }
        public bool IsWithdrawn { get; set; }
        public bool IsNotWithdrawn => !IsWithdrawn;
        
        public static implicit operator VacancyApplication(ApplicationReview applicationReview)
        {
            var projection = new VacancyApplication
            {
                CandidateId = applicationReview.CandidateId,
                Status = applicationReview.Status,
                SubmittedDate = applicationReview.SubmittedDate,
                ApplicationReviewId = applicationReview.Id,
                IsWithdrawn = applicationReview.IsWithdrawn,
                DisabilityStatus = ApplicationReviewDisabilityStatus.Unknown,
                Selected = false
            };

            if (applicationReview.IsWithdrawn == false)
            {
                projection.FirstName = applicationReview.Application.FirstName;
                projection.LastName = applicationReview.Application.LastName;
                projection.DateOfBirth = applicationReview.Application.BirthDate;
                projection.DisabilityStatus = applicationReview.Application.DisabilityStatus ?? ApplicationReviewDisabilityStatus.Unknown;
            }
            return projection;
        }

        public bool Selected { get; set; }
    }
}
