using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.TagHelpers
{
    public class WhenGettingReviewSectionState
    {
        [Test]
        [MoqInlineAutoData("Valid", "")]
        [MoqInlineAutoData("Incomplete", "app-check-answers__key--empty")]
        [MoqInlineAutoData("Review", "app-check-answers__key--review")]
        [MoqInlineAutoData("InvalidIncomplete", "app-check-answers__key--error")]
        public void Then_The_Correct_Css_Class_Is_Returned(string sectionState, string expectedClass, Esfa.Recruit.Employer.Web.TagHelpers.FieldReviewHelper helper)
        {
            //Act
            var actual = helper.GetReviewSectionClass(sectionState);
            
            //Assert
            actual.Should().Be(expectedClass);
        }
    }
}