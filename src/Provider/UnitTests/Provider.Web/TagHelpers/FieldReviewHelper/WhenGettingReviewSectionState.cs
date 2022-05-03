using Esfa.Recruit.Provider.Web.ViewModels.VacancyPreview;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.TagHelpers.FieldReviewHelper
{
    public class WhenGettingReviewSectionState
    {
        [Test]
        [MoqInlineAutoData("Valid", "")]
        [MoqInlineAutoData("Incomplete", "app-preview-section--empty")]
        [MoqInlineAutoData("Review", "app-preview-section--review")]
        [MoqInlineAutoData("InvalidIncomplete", "app-preview-section--error")]
        public void Then_The_Correct_Css_Class_Is_Returned(string sectionState, string expectedClass, Recruit.Provider.Web.TagHelpers.FieldReviewHelper helper)
        {
            //Act
            var actual = helper.GetReviewSectionClass(sectionState);
            
            //Assert
            actual.Should().Be(expectedClass);
        }
    }
}