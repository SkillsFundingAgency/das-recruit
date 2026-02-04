using System.Linq;
using Esfa.Recruit.Provider.Web.Controllers.Reports;
using Esfa.Recruit.Provider.Web.Orchestrators.Reports;
using Esfa.Recruit.Provider.Web.ViewModels.Reports.ReportDashboard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NUnit.Framework;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Controllers.Reports
{
    public class ReportDashboardControllerTests
    {
        [Test]
        public async Task Dashboard_Should_Populate_SuccessBanner_When_TempData_NewReportId_Matches_Report()
        {
            // Arrange
            var expectedReportId = Guid.NewGuid();
            var expectedReportName = "My new report";

            var vmFromOrchestrator = new ReportsDashboardViewModel
            {
                Ukprn = 123,
                Reports = new[]
                {
                    new ReportRowViewModel { ReportId = expectedReportId, ReportName = expectedReportName }
                }.AsEnumerable()
            };

            var mockOrchestrator = new Mock<IReportDashboardOrchestrator>();
            mockOrchestrator
                .Setup(x => x.GetDashboardViewModel(It.IsAny<long>()))
                .ReturnsAsync(vmFromOrchestrator);

            var controller = new ReportDashboardController(mockOrchestrator.Object);
            controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            controller.TempData["NewReportId"] = expectedReportId.ToString();

            // Act
            var result = await controller.Dashboard(123);

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = (ViewResult)result;
            Assert.That(viewResult.Model, Is.InstanceOf<ReportsDashboardViewModel>());
            var model = (ReportsDashboardViewModel)viewResult.Model;

            Assert.That(model.ShowSuccessBanner, Is.True);
            Assert.That(model.SuccessReportName, Is.EqualTo(expectedReportName));
        }
    }
}