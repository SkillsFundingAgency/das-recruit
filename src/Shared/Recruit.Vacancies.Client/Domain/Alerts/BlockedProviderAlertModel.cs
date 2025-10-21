using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Domain.Alerts;
public record BlockedProviderAlertModel
{
    public List<string> ClosedVacancies { get; set; } = [];
    public List<string> BlockedProviderNames { get; set; } = [];
}