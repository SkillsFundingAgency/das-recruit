using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Esfa.Recruit.Shared.Web.TagHelpers;

[HtmlTargetElement(TagName)]
[OutputElementHint("div")]
public class GovUkInsetTagHelper: TagHelper
{
    private const string TagName = "govuk-inset";
    public string Title { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Content.AppendHtml(RenderContent(await output.GetChildContentAsync()));
    }

    private static TagBuilder RenderContent(TagHelperContent content)
    {
        var div = new TagBuilder("div");
        div.AddCssClass("govuk-inset-text govuk-body");
        div.InnerHtml.AppendHtml(content);
        return div;
    }
}