using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.ApplicationReview
{
    public class ApplicationReviewSummary
    {
        public long VacancyReference { get; set; }
        public ApplicationReviewStatus Status { get; set; }
        public bool IsWithdrawn { get; set; }
    }
}
