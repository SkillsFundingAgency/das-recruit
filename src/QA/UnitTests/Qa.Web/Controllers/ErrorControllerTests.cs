using System.Net;
using Esfa.Recruit.Qa.Web.Controllers;
using Esfa.Recruit.Qa.Web.Models.Error;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace UnitTests.Qa.Web.Controllers
{
    [TestFixture]
    public class ErrorControllerTests
    {
        private ErrorController _sut;
        private Mock<IConfiguration> _mockConfiguration;
        private Mock<IConfigurationSection> _mockDfESignInSection;
        private Mock<IConfigurationSection> _mockResourceEnvSection;
        private Mock<ILogger<ErrorController>> _mockLogger;

        [SetUp]
        public void SetUp()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockDfESignInSection = new Mock<IConfigurationSection>();
            _mockResourceEnvSection = new Mock<IConfigurationSection>();
            _mockLogger = new Mock<ILogger<ErrorController>>();
        }

        [TestCase("test", "https://test-services.signin.education.gov.uk/approvals/select-organisation?action=request-service", true)]
        [TestCase("pp", "https://test-services.signin.education.gov.uk/approvals/select-organisation?action=request-service", true)]
        [TestCase("local", "https://test-services.signin.education.gov.uk/approvals/select-organisation?action=request-service", false)]
        [TestCase("prd", "https://services.signin.education.gov.uk/approvals/select-organisation?action=request-service", false)]
        public void WhenStatusCodeIs403Then403ViewIsReturned(string env, string helpLink, bool useDfESignIn)
        {
            //arrange
            _mockDfESignInSection.Setup(a => a.Value).Returns(useDfESignIn.ToString());
            _mockResourceEnvSection.Setup(a => a.Value).Returns(env);

            _mockConfiguration.Setup(a => a.GetSection("UseDfESignIn")).Returns(_mockDfESignInSection.Object);
            _mockConfiguration.Setup(a => a.GetSection("ResourceEnvironmentName")).Returns(_mockResourceEnvSection.Object);

            _sut = new ErrorController(_mockLogger.Object, _mockConfiguration.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            //sut
            var result = (ViewResult)_sut.Error(403);

            //assert
            Assert.That(result, Is.Not.Null);

            var actualModel = result.Model as Error403ViewModel;
            Assert.That(actualModel?.HelpPageLink, Is.EqualTo(helpLink));
            Assert.That(actualModel?.UseDfESignIn, Is.EqualTo(useDfESignIn));
            Assert.That(_sut.Response.StatusCode, Is.EqualTo((int)HttpStatusCode.Forbidden));
        }
    }
}
