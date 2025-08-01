using Esfa.Recruit.Shared.Web.TagHelpers;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Shared.Web.TagHelpers;

public class GovUkBannerHeadingTagHelperTests: TagHelperTestsBase
{
    [Test]
    public async Task Output_Renders_Correctly()
    {
        // arrange
        var sut = new GovUkBannerHeadingTagHelper();

        // act
        await sut.ProcessAsync(TagHelperContext, TagHelperOutput);

        // assert
        TagHelperOutput.AsString().Should().Be("<h3 class=\"HtmlEncode[[govuk-notification-banner__heading]]\">default content</h3>");
    }
}