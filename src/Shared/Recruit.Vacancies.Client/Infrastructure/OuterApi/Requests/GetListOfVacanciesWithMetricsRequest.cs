using System;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;

public class GetListOfVacanciesWithMetricsRequest : IGetApiRequest
{
    private readonly DateTime _startDate;
    private readonly DateTime _endDate;
    public GetListOfVacanciesWithMetricsRequest(DateTime startDate, DateTime endDate)
    {
        _startDate = startDate;
        _endDate = endDate;
    }

    public string GetUrl => $"metrics/vacancies?startDate={_startDate:yyyy-MM-ddTHH:mm:ss}&endDate={_endDate:yyyy-MM-ddTHH:mm:ss}";
}