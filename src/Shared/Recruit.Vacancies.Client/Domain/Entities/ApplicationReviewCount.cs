namespace Esfa.Recruit.Vacancies.Client.Domain.Entities
{
    public class ApplicationReviewCount
    {
        public ApplicationReviewsCountGroupKey Id { get; set; }
        public int Count { get; set; }

        public class ApplicationReviewsCountGroupKey
        {
            public long VacancyReference { get; set; }
            public ApplicationReviewStatus Status { get; set; }
        }
    }
}
