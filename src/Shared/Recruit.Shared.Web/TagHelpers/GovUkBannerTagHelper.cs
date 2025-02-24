using System;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Esfa.Recruit.Shared.Web.TagHelpers;

public enum BannerStyle
{
    Success
}

[HtmlTargetElement(TagName)]
[OutputElementHint("div")]
public class GovUkBannerTagHelper: TagHelper
{
    public const string TagName = "govuk-banner";
    public string Title { get; set; }
    public BannerStyle? Type { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.AddClass("govuk-notification-banner", HtmlEncoder.Default);
        
        switch (Type)
        {
            case BannerStyle.Success:
                {
                    output.AddClass("govuk-notification-banner--success", HtmlEncoder.Default);
                    Title ??= "Success";
                    break;
                }
            default:
                {
                    output.SuppressOutput();
                    return;
                }
        }

        output.Content.AppendHtml(RenderHeader(Title));
        output.Content.AppendHtml(RenderContent(await output.GetChildContentAsync()));

        output.Attributes.Add("role", "alert");
        output.Attributes.Add("aria-labelledby", "govuk-notification-banner-title");
        output.Attributes.Add("data-module", "govuk-notification-banner");
    }

    private static TagBuilder RenderContent(TagHelperContent content)
    {
        var div = new TagBuilder("div");
        div.AddCssClass("govuk-notification-banner__content");
        div.InnerHtml.AppendHtml(content);
        return div;
    }

    private static TagBuilder RenderHeader(string title)
    {
        var header = new TagBuilder("h2");
        header.AddCssClass("govuk-notification-banner__title");
        header.InnerHtml.AppendHtml(title);
        
        var div = new TagBuilder("div");
        div.AddCssClass("govuk-notification-banner__header");
        div.InnerHtml.AppendHtml(header);
        return div;
    }
}