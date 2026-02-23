using System.Linq;
using Esfa.Recruit.Shared.Web.TagHelpers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Shared.Web.TagHelpers;

public class RaaTagsTagHelperTests: TagHelperTestsBase
{
    [Test]
    public async Task RaaTags_TagHelper_Renders_Default_Output()
    {
        // arrange
        var sut = new RaaTagsTagHelper();

        // act
        await sut.ProcessAsync(TagHelperContext, TagHelperOutput);

        // assert
        TagHelperOutput.AsString().Should().Be("""<strong class="HtmlEncode[[govuk-tag]]">default content</strong>""");
    }
    
    [Test]
    public async Task RaaTags_TagHelper_Renders_Css()
    {
        // arrange
        var sut = new RaaTagsTagHelper
        {
            Class = " class1 class2"
        };

        // act
        await sut.ProcessAsync(TagHelperContext, TagHelperOutput);

        // assert
        TagHelperOutput.AsString().Should().Be("""<strong class="govuk-tag class1 class2">default content</strong>""");
    }
    
    [Test]
    public async Task FoundationTag_TagHelper_Renders_Output()
    {
        // arrange
        var sut = new FoundationTagTagHelper();

        // act
        await sut.ProcessAsync(TagHelperContext, TagHelperOutput);

        // assert
        TagHelperOutput.AsString().Should().Be("""<strong class="govuk-tag--pink govuk-tag">Foundation</strong>""");
    }
    
    [Test]
    public async Task ApiSubmittedTag_TagHelper_Renders_Output()
    {
        // arrange
        var sut = new ApiSubmittedTagHelper();

        // act
        await sut.ProcessAsync(TagHelperContext, TagHelperOutput);

        // assert
        TagHelperOutput.AsString().Should().Be("""<strong class="govuk-tag--yellow govuk-tag">API submitted</strong>""");
    }
}

public class StatusTagHelperTests: TagHelperTestsBase
{
    [TestCase(VacancyStatus.Draft, "<strong class=\"govuk-tag govuk-tag--grey\">Draft</strong>")]
    [TestCase(VacancyStatus.Submitted, "<strong class=\"govuk-tag govuk-tag--blue\">Submitted</strong>")]
    [TestCase(VacancyStatus.Live, "<strong class=\"govuk-tag govuk-tag--turquoise\">Live</strong>")]
    public async Task VacancyStatus_Renders_Canonical_Class(VacancyStatus status, string expected)
    {
        // Arrange
        var sut = new StatusTagHelper { VacancyStatusValue = status };

        // Act
        await sut.ProcessAsync(TagHelperContext, TagHelperOutput);

        // Assert
        TagHelperOutput.AsString().Should().Be(expected);
    }

    [TestCase(ApplicationReviewStatus.New, "<strong class=\"govuk-tag govuk-tag--light-blue\">New</strong>")]
    [TestCase(ApplicationReviewStatus.Unsuccessful, "<strong class=\"govuk-tag govuk-tag--orange\">Unsuccessful</strong>")]
    [TestCase(ApplicationReviewStatus.Interviewing, "<strong class=\"govuk-tag govuk-tag--purple\">Interviewing</strong>")]
    public async Task ApplicationStatus_Renders_Canonical_Class(ApplicationReviewStatus status, string expected)
    {
        // Arrange
        var sut = new StatusTagHelper { ApplicationStatusValue = status };

        // Act
        await sut.ProcessAsync(TagHelperContext, TagHelperOutput);

        // Assert
        TagHelperOutput.AsString().Should().Be(expected);
    }
}