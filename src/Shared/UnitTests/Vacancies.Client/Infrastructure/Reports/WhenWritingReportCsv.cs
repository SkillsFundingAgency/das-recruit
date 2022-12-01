using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Reports;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.Reports
{
    public class WhenWritingReportCsv
    {
        [Test]
        [MoqInlineAutoData(ReportStatus.Failed)]
        [MoqInlineAutoData(ReportStatus.New)]
        [MoqInlineAutoData(ReportStatus.InProgress)]
        public void Then_If_The_Report_Is_Not_In_The_Correct_Status_Then_Not_Downloaded(
            ReportStatus status,
            Report report,
            [Frozen] Mock<IReportStrategy> reportStrategy,
            [Frozen] Mock<IReportRepository> reportRepository,
            [Frozen] Mock<ICsvBuilder> csvBuilder)
        {
            //Arrange
            IReportStrategy ReportStrategyAccessor(ReportType type) => reportStrategy.Object;
            report.Query = string.Empty;
            report.Status = status;
            var reportService = new ReportService(Mock.Of<ILogger<ReportService>>(), reportRepository.Object,
                ReportStrategyAccessor, Mock.Of<ITimeProvider>(), csvBuilder.Object);

            //Act Assert
            Assert.ThrowsAsync<Exception>( () => reportService.WriteReportAsCsv(new MemoryStream(), report));
            csvBuilder.Verify(x=>x.WriteCsvToStream(It.IsAny<Stream>(), It.IsAny<JArray>(), It.IsAny<IEnumerable<KeyValuePair<string,string>>>(), It.IsAny<Func<string, ReportDataType>>()), Times.Never);
        }
        
        [Test, MoqAutoData]
        public async Task Then_If_The_Query_Is_Empty_Then_The_Report_Is_Written_From_The_Stored_Data(
            JArray reportData, 
            Report report,
            [Frozen] Mock<IReportStrategy> reportStrategy,
            [Frozen] Mock<IReportRepository> reportRepository,
            [Frozen] Mock<ICsvBuilder> csvBuilder)
        {
            //Arrange
            IReportStrategy ReportStrategyAccessor(ReportType type) => reportStrategy.Object;
            report.Query = string.Empty;
            report.Status = ReportStatus.Generated;
            report.Data = JsonConvert.SerializeObject(reportData);

            var reportService = new ReportService(Mock.Of<ILogger<ReportService>>(), reportRepository.Object,
                ReportStrategyAccessor, Mock.Of<ITimeProvider>(), csvBuilder.Object);

            //Act
            await reportService.WriteReportAsCsv(new MemoryStream(), report);
            
            //Assert
            csvBuilder.Verify(x=>x.WriteCsvToStream(It.IsAny<Stream>(), reportData, report.Headers, reportStrategy.Object.ResolveFormat));
        }
        
        [Test, MoqAutoData]
        public async Task Then_If_The_Query_Is_Not_Empty_Then_The_Report_Is_Written_From_The_Stored_Data(
            JArray reportData, 
            Report report,
            [Frozen] Mock<IReportStrategy> reportStrategy,
            [Frozen] Mock<IReportRepository> reportRepository,
            [Frozen] Mock<ICsvBuilder> csvBuilder)
        {
            //Arrange
            reportStrategy.Setup(x => x.GetApplicationReviewsRecursiveAsync(report.Query))
                .ReturnsAsync(JsonConvert.SerializeObject(reportData));
            IReportStrategy ReportStrategyAccessor(ReportType type) => reportStrategy.Object;
            report.Status = ReportStatus.Generated;
            report.Data = null;

            var reportService = new ReportService(Mock.Of<ILogger<ReportService>>(), reportRepository.Object,
                ReportStrategyAccessor, Mock.Of<ITimeProvider>(), csvBuilder.Object);

            //Act
            await reportService.WriteReportAsCsv(new MemoryStream(), report);
            
            //Assert
            csvBuilder.Verify(x=>x.WriteCsvToStream(It.IsAny<Stream>(), reportData, report.Headers, reportStrategy.Object.ResolveFormat));
        }
    }
}