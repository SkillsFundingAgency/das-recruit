using System.Security.Claims;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Controllers;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Orchestrators;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Controllers
{
    public class DashboardControllerTests
    {
        
        [Test, MoqAutoData]
        public async Task Then_View_Returned_ProviderDashboard(
            string userName,
            int ukprn,
            DashboardOrchestrator dashboardOrchestrator)
        {
            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

            var controller = new DashboardController(dashboardOrchestrator)
            {
                TempData = tempData
            };

            var user = new ClaimsPrincipal(new ClaimsIdentity(
                new[]
                {
                    new Claim(ProviderRecruitClaims.IdamsUserNameClaimTypeIdentifier,userName),
                    new Claim(ProviderRecruitClaims.IdamsUserUkprnClaimsTypeIdentifier,"10000001")
                }
            ));
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // act
            var expected = await dashboardOrchestrator.GetDashboardViewModelAsync(user.ToVacancyUser());
            var actual = await  controller.Dashboard() as ViewResult;

            // assert
            Assert.That(actual, Is.Not.Null);
            actual.Model.Should().BeEquivalentTo(expected);
        }
    }
}