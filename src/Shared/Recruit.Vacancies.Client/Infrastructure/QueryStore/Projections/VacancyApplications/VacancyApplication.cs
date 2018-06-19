using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications
{
    public class VacancyApplication
    {
        public DateTime SubmittedDate { get; set; }
        public ApplicationReviewStatus Status { get; set; }
        public string CandidateName { get; set; }
        public Guid ApplicationReviewId { get; set; }
    }
}
