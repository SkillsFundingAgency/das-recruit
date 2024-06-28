using System;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;

public class GetVacancyMetricsRequest : IGetApiRequest
{
    private readonly long _vacancyReference;
    private readonly DateTime _startDate;
    private readonly DateTime _endDate;

    public GetVacancyMetricsRequest(long vacancyReference, DateTime startDate, DateTime endDate)
    {
        _vacancyReference = vacancyReference;
        _startDate = startDate;
        _endDate = endDate;
    }

    public string GetUrl => $"metrics/vacancy/{_vacancyReference}?startDate={_startDate}&endDate={_endDate}";
}