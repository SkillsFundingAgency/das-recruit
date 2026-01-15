using System;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests.Reports;
public record GetReportApiRequest(Guid ReportId) : IGetApiRequest
{
    public string GetUrl => $"reports/{ReportId}";
}