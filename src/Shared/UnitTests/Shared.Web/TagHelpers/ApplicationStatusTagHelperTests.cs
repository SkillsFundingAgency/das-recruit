using Esfa.Recruit.Shared.Web.TagHelpers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Shared.Web.TagHelpers;

public class ApplicationStatusTagHelperTests : TagHelperTestsBase
{
    [TestCase(ApplicationReviewStatus.New, "<strong class=\"govuk-tag govuk-tag--light-blue\">New</strong>")]
    [TestCase(ApplicationReviewStatus.Unsuccessful, "<strong class=\"govuk-tag govuk-tag--orange\">Unsuccessful</strong>")]
    [TestCase(ApplicationReviewStatus.Interviewing, "<strong class=\"govuk-tag govuk-tag--purple\">Interviewing</strong>")]
    public async Task ApplicationStatus_Renders_Canonical_Class(ApplicationReviewStatus status, string expected)
    {
        // Arrange
        var sut = new ApplicationStatusTagHelper { ApplicationStatus = status };

        // Act
        await sut.ProcessAsync(TagHelperContext, TagHelperOutput);

        // Assert
        TagHelperOutput.AsString().Should().Be(expected);
    }
}