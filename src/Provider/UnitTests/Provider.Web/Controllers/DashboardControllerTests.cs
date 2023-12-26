using System.Security.Claims;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Controllers;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.ViewModels.Dashboard;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
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
            const string testUrl = "https://tempuri.org";
            var configuration = new Mock<IConfiguration>();
            configuration.Setup(c => c["ProviderSharedUIConfiguration:DashboardUrl"]).Returns(testUrl);
            configuration.Setup(c => c["TraineeshipCutOffDate"]).Returns("2023-Dec-25");
            var controller = new DashboardController(null, new ServiceParameters("Traineeship"), configuration.Object);

            var actual = await  controller.Dashboard() as RedirectResult;

            Assert.IsNotNull(actual);
            actual.Permanent.Should().BeTrue();
            actual.Url.Should().Be(testUrl);
        }
    }
}

