using Esfa.Recruit.Vacancies.Client.Domain.Alerts;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
public record GetAlertsByUkprnApiResponse
{
    public ProviderTransferredVacanciesAlertModel ProviderTransferredVacanciesAlert { get; set; } = new();
    public WithdrawnVacanciesAlertModel WithdrawnVacanciesAlert { get; set; } = new();
}