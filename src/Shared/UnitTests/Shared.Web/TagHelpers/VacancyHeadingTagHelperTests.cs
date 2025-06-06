using System.Collections.Generic;
using System.Text.Encodings.Web;
using Esfa.Recruit.Shared.Web.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Shared.Web.TagHelpers;

public class VacancyHeadingTagHelperTests
{
    private TagHelperContext _tagHelperContext;
    private TagHelperOutput _tagHelperOutput;
    
    [SetUp]
    public void SetUp()
    {
        _tagHelperContext = new TagHelperContext([], new Dictionary<object, object>(), "id");
        _tagHelperOutput = new TagHelperOutput(VacancyHeadingTagHelper.TagName, [], Func);
        return;

        static Task<TagHelperContent> Func(bool result, HtmlEncoder encoder)
        {
            var tagHelperContent = new DefaultTagHelperContent();
            tagHelperContent.SetHtmlContent(string.Empty);
            return Task.FromResult<TagHelperContent>(tagHelperContent);
        }
    }
    
    [Test]
    public async Task Classes_Are_Added_To_The_Tag()
    {
        // arrange
        var sut = new VacancyHeadingTagHelper
        {
            Class = "class1 class2",
        };

        // act
        await sut.ProcessAsync(_tagHelperContext, _tagHelperOutput);

        // assert
        _tagHelperOutput.AsString().Should().StartWith("<h1 class=\"govuk-heading-xl class1 class2\">");
    }
    
    
    [Test]
    public async Task Renders_As_H1()
    {
        // arrange
        var sut = new VacancyHeadingTagHelper();

        // act
        await sut.ProcessAsync(_tagHelperContext, _tagHelperOutput);

        // assert
        _tagHelperOutput.AsString().Should().StartWith("<h1 ");
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
        await sut.ProcessAsync(_tagHelperContext, _tagHelperOutput);

        // assert
        _tagHelperOutput.AsString().Should().Contain("<span class=\"HtmlEncode[[govuk-caption-l]]\">vacancy title</span>");
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
        await sut.ProcessAsync(_tagHelperContext, _tagHelperOutput);

        // assert
        _tagHelperOutput.AsString().Should().Contain("heading text</h1>");
    }
}