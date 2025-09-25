using Esfa.Recruit.Shared.Web.TagHelpers;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Shared.Web.TagHelpers;

public class GovUkBannerTagHelperTests: TagHelperTestsBase
{
    [Test]
    public async Task Output_Is_Suppresed_When_Banner_Type_Ommitted()
    {
        // arrange
        var sut = new GovUkBannerTagHelper();

        // act
        await sut.ProcessAsync(TagHelperContext, TagHelperOutput);

        // assert
        TagHelperOutput.AsString().Should().BeEmpty();
    }
    
    [TestCase(BannerStyle.Success, "Success")]
    [TestCase(BannerStyle.Important, "Important")]
    public async Task Output_Contains_Default_Header_For_Type(BannerStyle? bannerStyle, string title)
    {
        // arrange
        var sut = new GovUkBannerTagHelper
        {
            Type = bannerStyle
        };

        // act
        await sut.ProcessAsync(TagHelperContext, TagHelperOutput);

        // assert
        TagHelperOutput.AsString().Should().Contain($">{title}<");
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
        await sut.ProcessAsync(TagHelperContext, TagHelperOutput);

        // assert
        TagHelperOutput.AsString().Should().Contain($">{title}<");
    }
    
    [Test]
    public async Task Output_Contains_Default_Content()
    {
        // arrange
        var sut = new GovUkBannerTagHelper { Type = BannerStyle.Success };

        // act
        await sut.ProcessAsync(TagHelperContext, TagHelperOutput);

        // assert
        TagHelperOutput.AsString().Should().Contain(">default content<");
    }
    
    [Test]
    public async Task Banners_Role_Is_Alert()
    {
        // arrange
        var sut = new GovUkBannerTagHelper { Type = BannerStyle.Success };

        // act
        await sut.ProcessAsync(TagHelperContext, TagHelperOutput);

        // assert
        TagHelperOutput.AsString().Should().Contain("role=\"HtmlEncode[[alert]]\"");
    }
}