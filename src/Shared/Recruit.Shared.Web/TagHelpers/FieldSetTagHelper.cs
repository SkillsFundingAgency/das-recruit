using System;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Esfa.Recruit.Shared.Web.TagHelpers;

[HtmlTargetElement(TagName)]
[OutputElementHint("fieldset")]
public class FieldSetTagHelper : TagHelper
{
    public const string TagName = "govuk-fieldset";
    
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "fieldset";
        output.TagMode = TagMode.StartTagAndEndTag;
        
        output.AddClass("govuk-fieldset", HtmlEncoder.Default);
    }
}

[HtmlTargetElement(TagName, ParentTag = FieldSetTagHelper.TagName)]
[OutputElementHint("fieldset")]
public class LegendTagHelper : TagHelper
{
    public const string TagName = "govuk-legend";
    
    [HtmlAttributeName("size")]
    public HeadingSize Size { get; set; } = HeadingSize.Medium;
    
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "legend";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.AddClass("govuk-fieldset__legend", HtmlEncoder.Default);
        output.AddClass("govuk-fieldset__legend--s", HtmlEncoder.Default);

        var content = await output.GetChildContentAsync();
        if (Size is HeadingSize.Custom)
        {
            output.Content.AppendHtml(content);
            return;
        }

        var heading = GetHeading(Size);
        heading.AddCssClass(Size.GetCssClass()!);
        var headingText = await output.GetChildContentAsync();
        heading.InnerHtml.AppendHtml(headingText);
        output.Content.AppendHtml(heading);
    }

    private static TagBuilder GetHeading(HeadingSize size)
    {
        return size switch
        {
            HeadingSize.ExtraLarge => new TagBuilder("h1"),
            HeadingSize.Large => new TagBuilder("h1"),
            HeadingSize.Medium => new TagBuilder("h2"),
            HeadingSize.Small => new TagBuilder("h3"),
            _ => throw new ArgumentOutOfRangeException(nameof(size), size, null)
        };
    }
}