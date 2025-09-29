using Esfa.Recruit.Vacancies.Client.Domain.Alerts;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
public record GetAlertsByAccountIdApiResponse
{
    public EmployerTransferredVacanciesAlertModel EmployerRevokedTransferredVacanciesAlert { get; set; } = new();
    public EmployerTransferredVacanciesAlertModel BlockedProviderTransferredVacanciesAlert { get; set; } = new();
    public BlockedProviderAlertModel BlockedProviderAlert { get; set; } = new();
    public WithdrawnVacanciesAlertModel WithDrawnByQaVacanciesAlert { get; set; } = new();
}
