using System.Linq;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Esfa.Recruit.Shared.Web.TagHelpers;

[HtmlTargetElement(TagName, TagStructure = TagStructure.NormalOrSelfClosing)]
public class VacancyHeadingTagHelper: TagHelper
{
    public const string TagName = "vacancy-heading";
    public string VacancyTitle { get; set; }
    public string Heading { get; set; }
    public string Class { get; set; }
    
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "h1";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.AddClass("govuk-heading-xl", HtmlEncoder.Default);
        AddCssClasses(output, Class);
        Render(output, VacancyTitle, Heading);
    }

    private static void AddCssClasses(TagHelperOutput output, string cssClasses)
    {
        string[] classes = cssClasses?.Split(' ').Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
        foreach (string cssClass in classes ?? [])
        {
            output.AddClass(cssClass, HtmlEncoder.Default);
        }
    }
    
    private static void Render(TagHelperOutput output, string title, string heading)
    {
        var span = new TagBuilder("span");
        span.AddCssClass("govuk-caption-l");
        span.InnerHtml.AppendHtml(title);

        output.Content.AppendHtml(span);
        output.Content.AppendHtml(heading);
    }
}