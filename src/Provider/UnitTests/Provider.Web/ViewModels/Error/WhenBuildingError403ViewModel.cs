using Esfa.Recruit.Provider.Web.ViewModels.Error;
using NUnit.Framework;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.ViewModels.Error
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
    }
}
