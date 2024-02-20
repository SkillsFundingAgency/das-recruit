using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Controllers;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Controllers
{
    public class DashboardControllerTests
    {
        [Test, MoqAutoData]
        public async Task Then_The_User_Is_Redirected_To_ProviderDashboard_For_Traineeship()
        {
            const string testUrl = "https://tempuri.org/";
            var configuration = new Mock<IConfiguration>();
            configuration.Setup(c => c["ProviderSharedUIConfiguration:DashboardUrl"]).Returns(testUrl);
            configuration.Setup(c => c["TraineeshipCutOffDate"]).Returns("2023-Dec-25");
            var controller = new DashboardController(null, new ServiceParameters("Traineeship"), configuration.Object);

            var actual = await  controller.Dashboard() as RedirectResult;

            Assert.That(actual, Is.Not.Null);
            actual.Permanent.Should().BeTrue();
            actual.Url.Should().Be($"{testUrl}account");
        }
    }
}

