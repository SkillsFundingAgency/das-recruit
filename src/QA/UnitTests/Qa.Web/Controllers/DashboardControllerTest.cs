using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Qa.Web.Controllers;
using Esfa.Recruit.Qa.Web.Orchestrators;
using Esfa.Recruit.Qa.Web.Security;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace UnitTests.Qa.Web.Controllers
{
    public class DashboardControllerTest
    {
        [Test, RecursiveMoqAutoData]
        public async Task When_UseDfESignIn_True_And_UnAuthorized_Then_Return_Redirect(
            string searchTerm,
            [Frozen] Mock<IQaVacancyClient> qaVacancyClient,
            [Frozen] Mock<ITimeProvider> iTimeProvider,
            [Frozen] Mock<UserAuthorizationService> userAuthService,
            [Frozen] Mock<IConfiguration> configuration,
            [Frozen] Mock<IConfigurationSection> configurationSection)
        {
            //arrange
            bool useDfESignIn = true;
            configurationSection.Setup(a => a.Value).Returns(useDfESignIn.ToString());
            configuration.Setup(a => a.GetSection("UseDfESignIn")).Returns(configurationSection.Object);

            //sut
            var controller = new DashboardController(
                new DashboardOrchestrator(qaVacancyClient.Object, iTimeProvider.Object, userAuthService.Object),
                configuration.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
            };
            var actionResult = controller.Index(searchTerm);

            //assert
            var actual = (RedirectToActionResult) await actionResult;
            actual.Should().NotBeNull();

            actual?.ControllerName.Should().Be("Home");
            actual?.ActionName.Should().Be("Index");
        }
    }
}
