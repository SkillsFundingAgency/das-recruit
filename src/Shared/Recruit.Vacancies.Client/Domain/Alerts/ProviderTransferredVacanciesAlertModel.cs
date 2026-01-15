using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Domain.Alerts;
public record ProviderTransferredVacanciesAlertModel
{
    public List<string> LegalEntityNames { get; set; } = [];
}