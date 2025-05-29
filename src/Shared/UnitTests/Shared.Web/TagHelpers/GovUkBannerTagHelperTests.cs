using System.Collections.Generic;
using System.Text.Encodings.Web;
using Esfa.Recruit.Shared.Web.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Shared.Web.TagHelpers;

public class GovUkBannerTagHelperTests
{
    private TagHelperContext _tagHelperContext;
    private TagHelperOutput _tagHelperOutput;

    [SetUp]
    public void SetUp()
    {
        _tagHelperContext = new TagHelperContext([], new Dictionary<object, object>(), "id");
        _tagHelperOutput = new TagHelperOutput(GovUkBannerTagHelper.TagName, [], Func);
        return;

        static Task<TagHelperContent> Func(bool result, HtmlEncoder encoder)
        {
            var tagHelperContent = new DefaultTagHelperContent();
            tagHelperContent.SetHtmlContent("default content");
            return Task.FromResult<TagHelperContent>(tagHelperContent);
        }
    }
    
    [Test]
    public async Task Output_Is_Suppresed_When_Banner_Type_Ommitted()
    {
        // arrange
        var sut = new GovUkBannerTagHelper();

        // act
        await sut.ProcessAsync(_tagHelperContext, _tagHelperOutput);

        // assert
        _tagHelperOutput.AsString().Should().BeEmpty();
    }
    
    [TestCase(BannerStyle.Success, "Success")]
    public async Task Output_Contains_Default_Header_For_Type(BannerStyle? bannerStyle, string title)
    {
        // arrange
        var sut = new GovUkBannerTagHelper
        {
            Type = bannerStyle
        };

        // act
        await sut.ProcessAsync(_tagHelperContext, _tagHelperOutput);

        // assert
        _tagHelperOutput.AsString().Should().Contain($">{title}<");
    }
    
    [TestCase(BannerStyle.Success, "New Title")]
    public async Task Output_Contains_Overridden_Header_For_Type(BannerStyle? bannerStyle, string title)
    {
        // arrange
        var sut = new GovUkBannerTagHelper
        {
            Title = title,
            Type = bannerStyle
        };

        // act
        await sut.ProcessAsync(_tagHelperContext, _tagHelperOutput);

        // assert
        _tagHelperOutput.AsString().Should().Contain($">{title}<");
    }
    
    [Test]
    public async Task Output_Contains_Default_Content()
    {
        // arrange
        var sut = new GovUkBannerTagHelper { Type = BannerStyle.Success };

        // act
        await sut.ProcessAsync(_tagHelperContext, _tagHelperOutput);

        // assert
        _tagHelperOutput.AsString().Should().Contain($">default content<");
    }
    
    [Test]
    public async Task Banners_Role_Is_Alert()
    {
        // arrange
        var sut = new GovUkBannerTagHelper { Type = BannerStyle.Success };

        // act
        await sut.ProcessAsync(_tagHelperContext, _tagHelperOutput);

        // assert
        _tagHelperOutput.AsString().Should().Contain("role=\"HtmlEncode[[alert]]\"");
    }
}