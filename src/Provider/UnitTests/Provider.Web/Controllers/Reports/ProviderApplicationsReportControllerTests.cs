using System.Security.Claims;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Controllers.Reports;
using Esfa.Recruit.Provider.Web.Orchestrators.Reports;
using Esfa.Recruit.Provider.Web.ViewModels.Reports;
using Esfa.Recruit.Provider.Web.ViewModels.Reports.ProviderApplicationsReport;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NUnit.Framework;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Controllers.Reports
{
    public class ProviderApplicationsReportControllerTests
    {
        [Test]
        public async Task Create_Post_Should_Set_TempData_NewReportId_And_Redirect()
        {
            // Arrange
            var mockOrchestrator = new Mock<IProviderApplicationsReportOrchestrator>();
            var controller = new ProviderApplicationsReportController(mockOrchestrator.Object);

            controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

            var claims = new[]
            {
                new Claim("sub", "user-id"),
                new Claim("http://schemas.portal.com/name", "username"),
                new Claim("http://schemas.portal.com/displayname", "Display Name"),
                new Claim("http://schemas.portal.com/mail", "email@example.com"),
                new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/ukprn", "12345678")
            };
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test")) }
            };

            var model = new ProviderApplicationsReportCreateEditModel { Ukprn = 12345678, DateRange = DateRangeType.Last7Days };
            var expectedReportId = Guid.NewGuid();
            mockOrchestrator
                .Setup(x => x.PostCreateViewModelAsync(It.IsAny<ProviderApplicationsReportCreateEditModel>(), It.IsAny<VacancyUser>()))
                .ReturnsAsync(expectedReportId);

            // Act
            var result = await controller.Create(model);

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToRouteResult>());
            var redirect = (RedirectToRouteResult)result;
            Assert.That(redirect.RouteName, Is.EqualTo(RouteNames.ReportDashboard_Get));
            Assert.That(redirect.RouteValues.ContainsKey("Ukprn"), Is.True);
            Assert.That(redirect.RouteValues["Ukprn"], Is.EqualTo(model.Ukprn));

            Assert.That(controller.TempData.ContainsKey("NewReportId"), Is.True);
            Assert.That(controller.TempData["NewReportId"], Is.EqualTo(expectedReportId.ToString()));
        }
    }
}