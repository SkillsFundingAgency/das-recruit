using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Reports;

namespace Esfa.Recruit.Vacancies.Client.Application.Services.Reports
{
    public interface IReportService
    {
        Task GenerateReportAsync(Guid reportId);
        Task WriteReportAsCsv(Stream stream, Domain.Entities.Report report);
        Task WriteApplicationSummaryReportsV1ToCsv(
            Stream stream,
            List<ApplicationSummaryCsvReportV1> reports);
        Task WriteApplicationSummaryReportsV2ToCsv(
            Stream stream,
            List<ApplicationSummaryCsvReportV2> reports);
    }
}