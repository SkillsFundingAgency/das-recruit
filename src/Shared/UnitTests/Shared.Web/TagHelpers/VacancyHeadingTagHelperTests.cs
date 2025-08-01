using Esfa.Recruit.Shared.Web.TagHelpers;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Shared.Web.TagHelpers;

public class VacancyHeadingTagHelperTests: TagHelperTestsBase
{
    [Test]
    public async Task Classes_Are_Added_To_The_Tag()
    {
        // arrange
        var sut = new VacancyHeadingTagHelper
        {
            Class = "class1 class2",
        };

        // act
        await sut.ProcessAsync(TagHelperContext, TagHelperOutput);

        // assert
        TagHelperOutput.AsString().Should().StartWith("<h1 class=\"govuk-heading-xl class1 class2\">");
    }
    
    
    [Test]
    public async Task Renders_As_H1()
    {
        // arrange
        var sut = new VacancyHeadingTagHelper();

        // act
        await sut.ProcessAsync(TagHelperContext, TagHelperOutput);

        // assert
        TagHelperOutput.AsString().Should().StartWith("<h1 ");
    }
    
    [Test]
    public async Task Renders_Vacancy_Title()
    {
        // arrange
        var sut = new VacancyHeadingTagHelper
        {
            VacancyTitle = "vacancy title"
        };

        // act
        await sut.ProcessAsync(TagHelperContext, TagHelperOutput);

        // assert
        TagHelperOutput.AsString().Should().Contain("<span class=\"HtmlEncode[[govuk-caption-l]]\">vacancy title</span>");
    }
    
    [Test]
    public async Task Renders_Heading()
    {
        // arrange
        var sut = new VacancyHeadingTagHelper
        {
            Heading = "heading text"
        };

        // act
        await sut.ProcessAsync(TagHelperContext, TagHelperOutput);

        // assert
        TagHelperOutput.AsString().Should().Contain("heading text</h1>");
    }
}