using Esfa.Recruit.Shared.Web.TagHelpers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Shared.Web.TagHelpers;

public class VacancyStatusTagHelperTests : TagHelperTestsBase
{
    [Test]
    public void All_Status_Values_Are_Covered_In_Switch()
    {
        var values = Enum.GetValues<VacancyStatus>();

        values.Should().HaveCount(8); // update if enum grows
    }

    [Test]
    public async Task Renders_Strong_Tag_With_Base_Class()
    {
        var sut = new VacancyStatusTagHelper
        {
            Status = VacancyStatus.Approved
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
        var sut = new VacancyStatusTagHelper
        {
            Status = null
        };

        await sut.ProcessAsync(TagHelperContext, TagHelperOutput);

        var html = TagHelperOutput.AsString();

        html.Should().NotContain("govuk-tag--");
    }

    [TestCase(VacancyStatus.Draft, "govuk-tag--grey", "Draft" )]
    [TestCase(VacancyStatus.Review, "govuk-tag--blue", "Ready for review" )]
    [TestCase(VacancyStatus.Submitted, "govuk-tag--blue", "Pending review" )]
    [TestCase(VacancyStatus.Referred, "govuk-tag--red", "Rejected by DfE" )]
    [TestCase(VacancyStatus.Rejected, "govuk-tag--red", "Rejected by employer" )]
    [TestCase(VacancyStatus.Live, "govuk-tag--turquoise", "Live" )]
    [TestCase(VacancyStatus.Closed, "govuk-tag--grey", "Closed" )]
    [TestCase(VacancyStatus.Approved, "govuk-tag--green","Approved" )]
    public async Task Applies_Correct_Modifier_And_Display_Text_For_Status_And_Employer_User(
        VacancyStatus status,
        string expectedModifier,
        string expectedText)
    {
        // Arrange
        var sut = new VacancyStatusTagHelper
        {
            Status = status,
            UserType = UserType.Employer
        };

        // Act
        await sut.ProcessAsync(TagHelperContext, TagHelperOutput);

        // Assert
        var html = TagHelperOutput.AsString();

        html.Should().Contain(expectedModifier, "each status maps to a specific GOV.UK tag modifier class");
        html.Should().Contain($">{expectedText}</strong>", "each status should render the correct display text");
    }

    [TestCase(VacancyStatus.Draft, "govuk-tag--grey", "Draft")]
    [TestCase(VacancyStatus.Review, "govuk-tag--blue", "Pending employer review")]
    [TestCase(VacancyStatus.Submitted, "govuk-tag--blue", "Pending DfE review")]
    [TestCase(VacancyStatus.Referred, "govuk-tag--red", "Rejected by DfE")]
    [TestCase(VacancyStatus.Rejected, "govuk-tag--red", "Rejected by employer")]
    [TestCase(VacancyStatus.Live, "govuk-tag--turquoise", "Live")]
    [TestCase(VacancyStatus.Closed, "govuk-tag--grey", "Closed")]
    [TestCase(VacancyStatus.Approved, "govuk-tag--green", "Approved")]
    public async Task Applies_Correct_Modifier_And_Display_Text_For_Status_And_Provider_User(
        VacancyStatus status,
        string expectedModifier,
        string expectedText)
    {
        // Arrange
        var sut = new VacancyStatusTagHelper
        {
            Status = status,
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