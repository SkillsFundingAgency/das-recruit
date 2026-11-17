using System;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;

public class GetListOfVacanciesWithMetricsRequest(DateTime startDate, DateTime endDate) : IGetApiRequest
{
    public string GetUrl => $"metrics/vacancies?startDate={startDate:yyyy-MM-ddTHH:mm:ss}&endDate={endDate:yyyy-MM-ddTHH:mm:ss}";
}