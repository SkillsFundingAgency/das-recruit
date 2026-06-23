using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Services.Reports;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using Esfa.Recruit.Vacancies.Client.Domain.Reports;
using Newtonsoft.Json.Linq;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Reports;

public class ReportService(ITimeProvider timeProvider, ICsvBuilder csvBuilder) : IReportService
{
    public Task WriteApplicationSummaryReportsV1ToCsv(
        Stream stream,
        List<ApplicationSummaryCsvReportV1> reports)
    {
        var rows = JArray.FromObject(reports);
        var headers = new List<KeyValuePair<string, string>>
        {
            new("Date", timeProvider.Now.ToUkTime().ToString("dd/MM/yyyy HH:mm:ss")),
            new("Total_Number_Of_Applications", reports.Count.ToString())
        };
        csvBuilder.WriteCsvToStream(stream, rows, headers, ResolveDataType);
        return Task.CompletedTask;
    }

    public Task WriteApplicationSummaryReportsV2ToCsv(
        Stream stream,
        List<ApplicationSummaryCsvReportV2> reports)
    {
        var rows = JArray.FromObject(reports);
        var headers = new List<KeyValuePair<string, string>>
        {
            new("Date", timeProvider.Now.ToUkTime().ToString("dd/MM/yyyy HH:mm:ss")),
            new("Total_Number_Of_Applications", reports.Count.ToString())
        };
        csvBuilder.WriteCsvToStream(stream, rows, headers, ResolveDataType);
        return Task.CompletedTask;
    }

    private static ReportDataType ResolveDataType(string columnName) =>
        columnName switch
        {
            nameof(ApplicationSummaryReport.ApplicationDate) => ReportDataType.DateTimeType,
            nameof(ApplicationSummaryReport.VacancyClosingDate) => ReportDataType.DateTimeType,
            _ => ReportDataType.StringType
        };
}