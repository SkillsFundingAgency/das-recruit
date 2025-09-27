using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Domain.Alerts;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
public record GetVacanciesByUkprnApiResponse
{
    public Info PageInfo { get; set; }
    public List<VacancySummary> VacancySummaries { get; set; }
    public ProviderTransferredVacanciesAlertModel ProviderTransferredVacanciesAlert { get; set; } = new();
    public WithdrawnVacanciesAlertModel WithdrawnVacanciesAlert { get; set; } = new();
}