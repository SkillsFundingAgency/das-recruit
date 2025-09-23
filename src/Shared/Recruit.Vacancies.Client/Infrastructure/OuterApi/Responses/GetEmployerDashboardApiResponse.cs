using Esfa.Recruit.Vacancies.Client.Domain.Alerts;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses
{
    public record GetEmployerDashboardApiResponse
    {
        public int NewApplicationsCount { get; set; }
        public int EmployerReviewedApplicationsCount { get; set; }
        public int SharedApplicationsCount { get; set; } = 0;
        public int AllSharedApplicationsCount { get; set; } = 0;
        public int SuccessfulApplicationsCount { get; set; } = 0;
        public int UnsuccessfulApplicationsCount { get; set; } = 0;
        public bool HasNoApplications { get; set; } = false;
        public int ClosedVacanciesCount { get; set; }
        public int DraftVacanciesCount { get; set; }
        public int ReviewVacanciesCount { get; set; }
        public int ReferredVacanciesCount { get; set; }
        public int LiveVacanciesCount { get; set; }
        public int SubmittedVacanciesCount { get; set; }
        public int ClosingSoonVacanciesCount { get; set; }
        public int ClosingSoonWithNoApplications { get; set; }
        public EmployerTransferredVacanciesAlertModel EmployerRevokedTransferredVacanciesAlert { get; set; } = new();
        public EmployerTransferredVacanciesAlertModel BlockedProviderTransferredVacanciesAlert { get; set; } = new();
        public BlockedProviderAlertModel BlockedProviderAlert { get; set; } = new();
        public WithdrawnVacanciesAlertModel WithDrawnByQaVacanciesAlert { get; set; } = new();
    }
}