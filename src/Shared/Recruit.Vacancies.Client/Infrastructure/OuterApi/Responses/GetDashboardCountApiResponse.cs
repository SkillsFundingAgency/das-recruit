namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses
{
    public record GetDashboardCountApiResponse
    {
        public int NewApplicationsCount { get; set; }
        public int EmployerReviewedApplicationsCount { get; set; }
        public int SharedApplicationsCount { get; set; } = 0;
        public int AllSharedApplicationsCount { get; set; } = 0;
        public int SuccessfulApplicationsCount { get; set; } = 0;
        public int UnsuccessfulApplicationsCount { get; set; } = 0;
        public bool HasNoApplications { get; set; } = false;
    }
}