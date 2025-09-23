using Esfa.Recruit.Vacancies.Client.Domain.Alerts;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses
{
    public record GetProviderDashboardApiResponse
    {
        public int NewApplicationsCount { get; set; }
        public int EmployerReviewedApplicationsCount { get; set; }
        public int SharedApplicationsCount { get; set; } = 0;
        public int AllSharedApplicationsCount { get; set; } = 0;
        public int SuccessfulApplicationsCount { get; set; } = 0;
        public int UnsuccessfulApplicationsCount { get; set; } = 0;
        public bool HasNoApplications { get; set; } = false;
        public int ClosedVacanciesCount { get; init; }
        public int DraftVacanciesCount { get; init; }
        public int ReviewVacanciesCount { get; init; }
        public int ReferredVacanciesCount { get; init; }
        public int LiveVacanciesCount { get; init; }
        public int SubmittedVacanciesCount { get; init; }
        public int ClosingSoonVacanciesCount { get; init; }
        public int ClosingSoonWithNoApplications { get; init; }
        public ProviderTransferredVacanciesAlertModel ProviderTransferredVacanciesAlert { get; set; } = new();
        public WithdrawnVacanciesAlertModel WithdrawnVacanciesAlert { get; set; } = new();
    }
}