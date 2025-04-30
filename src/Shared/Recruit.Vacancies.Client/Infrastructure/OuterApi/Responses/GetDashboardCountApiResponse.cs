namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses
{
    public record GetDashboardCountApiResponse
    {
        public int NewApplicationsCount { get; set; }
        public int EmployerReviewedApplicationsCount { get; set; }
    }
}