using System.Security.Claims;
using AutoFixture.NUnit3;
using Esfa.Recruit.Qa.Web.Controllers;
using Esfa.Recruit.Qa.Web.ViewModels.Home;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;

namespace UnitTests.Qa.Web.Controllers
{
    public class HomeControllerTest
    {
        private HomeController _controller;
        private ClaimsPrincipal _claimsPrincipal;
        private Mock<IConfiguration> _mockConfiguration;
        private Mock<IConfigurationSection> _mockSection;
        private Mock<HttpContext> _mockHttpContext;

        [SetUp]
        public void Setup()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockSection = new Mock<IConfigurationSection>();
            _mockHttpContext = new Mock<HttpContext>();
            _claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("name", "someName")
            }, "mock"));
        }

        [Test, AutoData]
        public void When_User_Authenticated_Then_Return_Redirect(bool useDfESignIn)
        {
            //arrange
            _mockHttpContext.Setup(c => c.User).Returns(_claimsPrincipal);
            _mockHttpContext.Setup(c => c.User.Identity.IsAuthenticated).Returns(true);

            _mockSection.Setup(a => a.Value).Returns(useDfESignIn.ToString());
            _mockConfiguration.Setup(a => a.GetSection("UseDfESignIn")).Returns(_mockSection.Object);

            _controller = new HomeController(_mockConfiguration.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = _mockHttpContext.Object }
            };

            //sut
            var actionResult = _controller.Index();

            //assert
            var actual = actionResult as RedirectToActionResult;
            actual.Should().NotBeNull();

            actual?.ActionName.Should().Be("Index");
            actual?.ControllerName.Should().Be("Dashboard");
        }

        [Test, AutoData]
        public void When_User_Not_Authenticated_Then_Return_View(bool useDfESignIn)
        {
            //arrange
            _mockHttpContext.Setup(c => c.User).Returns(_claimsPrincipal);
            _mockHttpContext.Setup(c => c.User.Identity.IsAuthenticated).Returns(false);

            _mockSection.Setup(a => a.Value).Returns(useDfESignIn.ToString());
            _mockConfiguration.Setup(a => a.GetSection("UseDfESignIn")).Returns(_mockSection.Object);

            _controller = new HomeController(_mockConfiguration.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = _mockHttpContext.Object }
            };

            //sut
            var actionResult = _controller.Index();

            //assert
            var actual = actionResult as ViewResult;
            actual.Should().NotBeNull();

            var model = actual?.Model as HomeIndexViewModel;
            model?.UseDfESignIn.Should().Be(useDfESignIn);
        }
    }
}
