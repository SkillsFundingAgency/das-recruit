using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;

public class GetVacanciesWithMetricsResponse
{
    public List<long> Vacancies { get; set; }
}