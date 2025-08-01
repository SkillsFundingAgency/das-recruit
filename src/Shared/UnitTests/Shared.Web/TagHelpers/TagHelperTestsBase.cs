using System.Collections.Generic;
using System.Text.Encodings.Web;
using Esfa.Recruit.Shared.Web.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Shared.Web.TagHelpers;

public abstract class TagHelperTestsBase
{
    protected TagHelperContext TagHelperContext;
    protected TagHelperOutput TagHelperOutput;
    protected virtual string DefaultContent => "default content";
    
    [SetUp]
    public void SetUp()
    {
        TagHelperContext = new TagHelperContext([], new Dictionary<object, object>(), "id");
        TagHelperOutput = new TagHelperOutput(GovUkBannerTagHelper.TagName, [], Func);
        return;

        Task<TagHelperContent> Func(bool result, HtmlEncoder encoder)
        {
            var tagHelperContent = new DefaultTagHelperContent();
            tagHelperContent.SetHtmlContent(DefaultContent);
            return Task.FromResult<TagHelperContent>(tagHelperContent);
        }
    }
}