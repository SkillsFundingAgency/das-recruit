using AutoFixture.NUnit3;
using Esfa.Recruit.Qa.Web.Controllers;
using Esfa.Recruit.Qa.Web.ViewModels.Home;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace UnitTests.Qa.Web.Controllers
{
    public class HomeControllerTest
    {
        [Test]
        public void When_UseDfESignIn_True_Then_Return_Model()
        {
            //arrange
            bool useDfESignIn = true;
            var configuration = new Mock<IConfiguration>();
            var configurationSection = new Mock<IConfigurationSection>();
            configurationSection.Setup(a => a.Value).Returns(useDfESignIn.ToString());
            configuration.Setup(a => a.GetSection("UseDfESignIn")).Returns(configurationSection.Object);

            //sut
            var controller = new HomeController(configuration.Object);
            var actionResult = controller.Index();

            //assert
            var actual = actionResult as ViewResult;
            actual.Should().NotBeNull();

            var model = actual?.Model as HomeIndexViewModel;
            model?.UseDfESignIn.Should().BeTrue();
        }

        [Test]
        public void When_UseDfESignIn_False_Then_Return_Redirect()
        {
            //arrange
            bool useDfESignIn = false;
            var configuration = new Mock<IConfiguration>();
            var configurationSection = new Mock<IConfigurationSection>();
            configurationSection.Setup(a => a.Value).Returns(useDfESignIn.ToString());
            configuration.Setup(a => a.GetSection("UseDfESignIn")).Returns(configurationSection.Object);

            //sut
            var controller = new HomeController(configuration.Object);
            var actionResult = controller.Index();

            //assert
            var actual = actionResult as RedirectToActionResult;
            actual.Should().NotBeNull();

            actual?.ControllerName.Should().Be("Dashboard");
            actual?.ActionName.Should().Be("Index");
        }
    }
}
