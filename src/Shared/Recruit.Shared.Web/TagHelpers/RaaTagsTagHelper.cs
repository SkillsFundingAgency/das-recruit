using System;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Esfa.Recruit.Shared.Web.TagHelpers;

[HtmlTargetElement(TagName)]
[OutputElementHint("strong")]
public class RaaTagsTagHelper : TagHelper
{
    public const string TagName = "govuk-tag";

    public string Class { get; set; }
    
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "strong";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.AddClass("govuk-tag", HtmlEncoder.Default);
        if (!string.IsNullOrWhiteSpace(Class))
        {
            var classes = Class.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
            foreach (string className in classes)
            {
                output.AddClass(className, HtmlEncoder.Default);
            }
        }

        var content = await GetContent(output);
        output.Content.AppendHtml(content);
    }

    protected virtual async Task<IHtmlContent> GetContent(TagHelperOutput output)
    {
        return await output.GetChildContentAsync();
    }
}

[HtmlTargetElement(TagName)]
public class FoundationTagTagHelper : RaaTagsTagHelper
{
    public new const string TagName = "govuk-tag-foundation";
    
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.AddClass("govuk-tag--pink", HtmlEncoder.Default);
        await base.ProcessAsync(context, output);
    }

    protected override Task<IHtmlContent> GetContent(TagHelperOutput output)
    {
        return Task.FromResult<IHtmlContent>(new HtmlString("Foundation"));
    }
}