using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Domain.Alerts;
public record EmployerTransferredVacanciesAlertModel
{
    public int TransferredVacanciesCount { get; init; }
    public List<string> TransferredVacanciesProviderNames { get; init; } = [];
}