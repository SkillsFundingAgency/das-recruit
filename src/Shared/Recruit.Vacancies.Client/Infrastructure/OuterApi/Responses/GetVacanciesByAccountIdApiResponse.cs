using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
public record GetVacanciesByAccountIdApiResponse
{
    public Info PageInfo { get; set; }
    public List<VacancySummary> VacancySummaries { get; set; }
}