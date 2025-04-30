namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses
{
    public record ApplicationReviewStats
    {
        public long VacancyReference { get; set; }
        public int Applications { get; set; }
        public int NewApplications { get; set; }
        public int SharedApplications { get; set; }
        public int SuccessfulApplications { get; set; }
        public int UnsuccessfulApplications { get; set; }
        public int EmployerReviewedApplications { get; set; }
    }
}