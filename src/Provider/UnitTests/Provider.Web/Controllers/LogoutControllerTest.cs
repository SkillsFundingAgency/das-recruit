using System.Diagnostics;
using AutoFixture;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Controllers
{
    public class LogoutControllerTest
    {
        private Fixture _fixture;
        private LogoutController _sut;
        private Mock<IConfiguration>? _configuration;
        private Mock<IOptions<ExternalLinksConfiguration>> _externalLinksConfig;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _configuration = new Mock<IConfiguration>();
            _externalLinksConfig = new Mock<IOptions<ExternalLinksConfiguration>>();
            _sut = new LogoutController(_externalLinksConfig.Object, _configuration.Object);
        }

        [Test]
        public void When_Dashboard_Called_Redirect()
        {
            // Arrange
            var url = _fixture.Create<string>();

            _configuration?.SetupGet(x => x[It.Is<string>(s => s == "ProviderSharedUIConfiguration:DashboardUrl")]).Returns(url);

            // Act
            var result = (RedirectResult)_sut.Dashboard();
            var actual = result;

            // Assert
            Debug.Assert(actual != null, nameof(actual) + " != null");
            actual.Url.Should().Be(url);
        }
    }
}
