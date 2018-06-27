using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Dashboard
{
    public class ApplicationReviewSummary
    {
        public long VacancyReference { get; set; }
        public ApplicationReviewStatus Status { get; set; }
    }
}
