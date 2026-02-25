using Esfa.Recruit.Shared.Web.TagHelpers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Shared.Web.TagHelpers;

public class ApplicationStatusTagHelperTests : TagHelperTestsBase
{
    [Test]
    public void All_Status_Values_Are_Covered_In_Switch()
    {
        var values = Enum.GetValues<ApplicationReviewStatus>();

        values.Should().HaveCount(11); // update if enum grows
    }

    [Test]
    public async Task Renders_Strong_Tag_With_Base_Class()
    {
        var sut = new ApplicationStatusTagHelper
        {
            ApplicationStatus = ApplicationReviewStatus.New
        };

        await sut.ProcessAsync(TagHelperContext, TagHelperOutput);

        var html = TagHelperOutput.AsString();

        html.Should().StartWith("<strong");
        html.Should().Contain("class=\"govuk-tag");
        html.Should().EndWith("</strong>");
    }

    [Test]
    public async Task Does_Not_Apply_Modifier_When_Status_Is_Null()
    {
        var sut = new ApplicationStatusTagHelper
        {
            ApplicationStatus = null
        };

        await sut.ProcessAsync(TagHelperContext, TagHelperOutput);

        var html = TagHelperOutput.AsString();

        html.Should().NotContain("govuk-tag--");
    }

    [TestCase(ApplicationReviewStatus.New, "govuk-tag--light-blue", "New")]
    [TestCase(ApplicationReviewStatus.InReview, "govuk-tag--yellow", "In review")]
    [TestCase(ApplicationReviewStatus.Unsuccessful, "govuk-tag--orange", "Unsuccessful")]
    [TestCase(ApplicationReviewStatus.EmployerUnsuccessful, "govuk-tag--orange", "Unsuccessful")]
    [TestCase(ApplicationReviewStatus.Shared, "govuk-tag--yellow", "Response Needed")]
    [TestCase(ApplicationReviewStatus.Interviewing, "govuk-tag--purple", "Interviewing")]
    [TestCase(ApplicationReviewStatus.EmployerInterviewing, "govuk-tag--pink", "Interviewing")]
    [TestCase(ApplicationReviewStatus.Successful, "govuk-tag--green", "Successful")]
    public async Task Applies_Correct_Modifier_And_Display_Text_For_Status_And_Employer_User(
        ApplicationReviewStatus status,
        string expectedModifier,
        string expectedText)
    {
        // Arrange
        var sut = new ApplicationStatusTagHelper
        {
            ApplicationStatus = status,
            UserType = UserType.Employer
        };

        // Act
        await sut.ProcessAsync(TagHelperContext, TagHelperOutput);

        // Assert
        var html = TagHelperOutput.AsString();

        html.Should().Contain(expectedModifier, "each status maps to a specific GOV.UK tag modifier class");
        html.Should().Contain($">{expectedText}</strong>", "each status should render the correct display text");
    }

    [TestCase(ApplicationReviewStatus.New, "govuk-tag--light-blue", "New")]
    [TestCase(ApplicationReviewStatus.InReview, "govuk-tag--yellow", "In review")]
    [TestCase(ApplicationReviewStatus.Unsuccessful, "govuk-tag--orange", "Unsuccessful")]
    [TestCase(ApplicationReviewStatus.EmployerUnsuccessful, "govuk-tag--orange", "Employer reviewed")]
    [TestCase(ApplicationReviewStatus.Shared, "govuk-tag--yellow", "Shared")]
    [TestCase(ApplicationReviewStatus.Interviewing, "govuk-tag--purple", "Interviewing")]
    [TestCase(ApplicationReviewStatus.EmployerInterviewing, "govuk-tag--pink", "Employer reviewed")]
    [TestCase(ApplicationReviewStatus.Successful, "govuk-tag--green", "Successful")]
    public async Task Applies_Correct_Modifier_And_Display_Text_For_Status_And_Provider_User(
        ApplicationReviewStatus status,
        string expectedModifier,
        string expectedText)
    {
        // Arrange
        var sut = new ApplicationStatusTagHelper
        {
            ApplicationStatus = status,
            UserType = UserType.Provider
        };

        // Act
        await sut.ProcessAsync(TagHelperContext, TagHelperOutput);

        // Assert
        var html = TagHelperOutput.AsString();

        html.Should().Contain(expectedModifier, "each status maps to a specific GOV.UK tag modifier class");
        html.Should().Contain($">{expectedText}</strong>", "each status should render the correct display text");
    }
}