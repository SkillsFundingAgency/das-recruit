using Esfa.Recruit.Qa.Web.Models.Error;
using NUnit.Framework;

namespace UnitTests.Qa.Web.ViewModels
{
    public class WhenBuildingError403ViewModel
    {
        [Test]
        public void Then_The_HelpPage_Link_Is_Correct_For_Production_Environment()
        {
            var actual = new Error403ViewModel("prd");

            Assert.That(actual.HelpPageLink, Is.Not.Null);
            Assert.That(actual.HelpPageLink, Is.EqualTo("https://services.signin.education.gov.uk/approvals/select-organisation?action=request-service"));
        }

        [Test]
        public void Then_The_HelpPage_Link_Is_Correct_For_Non_Production_Environment()
        {
            var actual = new Error403ViewModel("test");

            Assert.That(actual.HelpPageLink, Is.Not.Null);
            Assert.That(actual.HelpPageLink, Is.EqualTo("https://test-services.signin.education.gov.uk/approvals/select-organisation?action=request-service"));
        }

        [TestCase("")]
        [TestCase(null)]
        public void Then_The_HelpPage_Link_Is_Correct_When_Environment_Is_Null(string env)
        {
            var actual = new Error403ViewModel(env);

            Assert.That(actual.HelpPageLink, Is.Not.Null);
            Assert.That(actual.HelpPageLink, Is.EqualTo("https://services.signin.education.gov.uk/approvals/select-organisation?action=request-service"));
        }
    }
}
