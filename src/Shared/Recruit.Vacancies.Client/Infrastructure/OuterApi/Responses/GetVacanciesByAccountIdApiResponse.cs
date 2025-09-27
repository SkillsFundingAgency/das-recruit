using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Domain.Alerts;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
public record GetVacanciesByAccountIdApiResponse
{
    public Info PageInfo { get; set; }
    public List<VacancySummary> VacancySummaries { get; set; }
    public EmployerTransferredVacanciesAlertModel EmployerRevokedTransferredVacanciesAlert { get; set; } = new();
    public EmployerTransferredVacanciesAlertModel BlockedProviderTransferredVacanciesAlert { get; set; } = new();
    public BlockedProviderAlertModel BlockedProviderAlert { get; set; } = new();
    public WithdrawnVacanciesAlertModel WithDrawnByQaVacanciesAlert { get; set; } = new();
}