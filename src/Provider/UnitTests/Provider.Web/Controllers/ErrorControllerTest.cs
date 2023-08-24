using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Controllers;
using Esfa.Recruit.Provider.Web.Models.Error;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Controllers
{
    [TestFixture]
    public class ErrorControllerTests
    {
        private ErrorController _sut;
        private Mock<IConfiguration> _mockConfiguration;
        private Mock<ILogger<ErrorController>> _logger;
        private Mock<IOptions<ExternalLinksConfiguration>> _externalLinks = new();
        private Mock<IRecruitVacancyClient> _vacancyClient = new();
        private Mock<ITrainingProviderSummaryProvider> _trainingProviderSummaryProvider = new();

        [SetUp]
        public void SetUp()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _logger = new Mock<ILogger<ErrorController>>();
            _sut = new ErrorController(
                _logger.Object,
                _externalLinks.Object,
                _vacancyClient.Object,
                _trainingProviderSummaryProvider.Object,
                _mockConfiguration.Object);
        }

        [Test]
        [TestCase("test", "https://test-services.signin.education.gov.uk/organisations")]
        [TestCase("pp", "https://test-services.signin.education.gov.uk/organisations")]
        [TestCase("local", "https://test-services.signin.education.gov.uk/organisations")]
        [TestCase("prd", "https://services.signin.education.gov.uk/organisations")]
        public void WhenStatusCodeIs403Then403ViewIsReturned(string env, string helpLink)
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            _sut = new ErrorController(
                _logger.Object,
                _externalLinks.Object,
                _vacancyClient.Object,
                _trainingProviderSummaryProvider.Object,
                _mockConfiguration.Object)
            {
                TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>())
                {
                    [TempDataKeys.IsBlockedProvider] = false
                }
            };
            _sut.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, "username")
                    }, "someAuthTypeName"))
                }
            };

            //arrange
            _mockConfiguration.Setup(x => x["ResourceEnvironmentName"]).Returns(env);

            var result = (ViewResult)_sut.Error(403);

            Assert.That(result, Is.Not.Null);
            var actualModel = result?.Model as Error403ViewModel;
            Assert.That(actualModel?.HelpPageLink, Is.EqualTo(helpLink));
        }

        [Test]
        public void WhenStatusCodeIs404Then404ViewIsReturned()
        {
            var result = (ViewResult)_sut.Error(404);
            result.ViewName.Should().Be("404");
        }

        [TestCase(null)]
        [TestCase(401)]
        [TestCase(405)]
        public void WhenStatusCodeIsNotHandledThenGenericErrorViewIsReturned(int errorCode)
        {
            var result = (ViewResult)_sut.Error(errorCode);
            result.ViewName.Should().BeNull();
        }
    }
}
