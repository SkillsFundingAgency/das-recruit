using System;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests.Reports;
public record GetReportDataApiRequest(Guid ReportId) : IGetApiRequest
{
    public string GetUrl => $"reports/generate/{ReportId}";
}