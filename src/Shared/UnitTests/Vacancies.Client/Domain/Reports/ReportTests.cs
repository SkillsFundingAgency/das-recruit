using System;
using System.Text.Json;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Reports;
using FluentAssertions;
using NUnit.Framework;
using DomainReport = Esfa.Recruit.Vacancies.Client.Domain.Reports.Report;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Domain.Reports
{
    [TestFixture]
    public class ReportTests
    {
        private static DomainReport BuildReport(ReportStatus status) => new()
        {
            Id = Guid.NewGuid(),
            Name = "Test Report",
            Type = ReportType.ProviderApplications,
            OwnerType = ReportOwnerType.Provider,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "test-user",
            UserId = "user-1",
            DownloadCount = 0,
            Status = status,
            DynamicCriteria = JsonSerializer.Serialize(new ReportCriteria { Ukprn = 12345678 })
        };

        [TestCase(ReportStatus.New)]
        [TestCase(ReportStatus.InProgress)]
        [TestCase(ReportStatus.Generated)]
        [TestCase(ReportStatus.Failed)]
        public void ToReportSummary_ShouldMapStatus(ReportStatus status)
        {
            var report = BuildReport(status);

            var summary = DomainReport.ToReportSummary(report);

            summary.Status.Should().Be(status);
        }

        [TestCase(ReportStatus.New)]
        [TestCase(ReportStatus.InProgress)]
        [TestCase(ReportStatus.Generated)]
        [TestCase(ReportStatus.Failed)]
        public void ToEntity_ShouldMapStatus(ReportStatus status)
        {
            var report = BuildReport(status);

            var entity = report.ToEntity(report);

            entity.Status.Should().Be(status);
        }
    }
}
