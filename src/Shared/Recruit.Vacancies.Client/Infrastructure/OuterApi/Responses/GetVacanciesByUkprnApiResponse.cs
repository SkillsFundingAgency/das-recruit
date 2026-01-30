using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
public record GetVacanciesByUkprnApiResponse
{
    public Info PageInfo { get; set; }
    public List<VacancySummary> VacancySummaries { get; set; }
}