using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Domain.Alerts;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Provider;

public class ProviderDashboard
{
    public IEnumerable<VacancySummary> Vacancies { get; set; }
    public int? TotalVacancies { get; set; } = null;
    public ProviderTransferredVacanciesAlertModel ProviderTransferredVacanciesAlert { get; set; } = new();
    public WithdrawnVacanciesAlertModel WithdrawnVacanciesAlert { get; set; } = new();
}