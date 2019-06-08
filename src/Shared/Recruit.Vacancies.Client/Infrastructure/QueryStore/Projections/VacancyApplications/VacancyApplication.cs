using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications
{
    public class VacancyApplication
    {
        private string _candidateName;

        public DateTime SubmittedDate { get; set; }
        public ApplicationReviewStatus Status { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CandidateName
        { 
            get
            {
                if (!string.IsNullOrEmpty(FirstName))
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

        public DateTime DateOfBirth { get; set; }
        public Guid ApplicationReviewId { get; set; }
        public ApplicationReviewDisabilityStatus DisabilityStatus { get; set; }
        public bool IsWithdrawn { get; set; }
        public bool IsNotWithdrawn => !IsWithdrawn;
    }
}
