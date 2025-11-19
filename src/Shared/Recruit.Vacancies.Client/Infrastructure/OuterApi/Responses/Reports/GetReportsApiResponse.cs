using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Domain.Reports;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses.Reports;
public record GetReportDataApiResponse
{
    public List<ApplicationSummaryReport> Reports { get; init; }
}