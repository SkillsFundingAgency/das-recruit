using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Esfa.Recruit.Shared.Web.TagHelpers;

[HtmlTargetElement(TagName)]
[OutputElementHint("div")]
public class RadioButtonGroupTagHelper : TagHelper
{
    public const string TagName = "govuk-radio-group";

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.AddClass("govuk-radios", HtmlEncoder.Default);
    }
}

[HtmlTargetElement(TagName, ParentTag = RadioButtonGroupTagHelper.TagName)]
[OutputElementHint("div")]
public class RadioButtonTagHelper(IHtmlGenerator htmlGenerator): TagHelper
{
    public const string TagName = "govuk-radio-button";
    
    [ViewContext, HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; }
    
    [HtmlAttributeName("id")]
    public string Id { get; set; }
    
    [HtmlAttributeName("value")]
    public object Value { get; set; }
    
    [HtmlAttributeName("for")]
    public ModelExpression For { get; set; }

    [HtmlAttributeName("conditional")]
    public string DataAriaControls { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.AddClass("govuk-radios__item", HtmlEncoder.Default);
        if (DataAriaControls is not null)
        {
            output.AddClass("govuk-checkboxes--conditional", HtmlEncoder.Default);
        }

        var radio = htmlGenerator.GenerateRadioButton(
            ViewContext,
            For?.ModelExplorer,
            For?.Name,
            Value,
            null,
            new { @class = "govuk-radios__input", id = Id });
        
        if (DataAriaControls is not null)
        {
            radio.MergeAttribute("data-aria-controls", DataAriaControls);
        }
        output.Content.AppendHtml(radio);

        var label = new TagBuilder("label");
        label.AddCssClass("govuk-label");
        label.AddCssClass("govuk-radios__label");
        label.Attributes.Add("for", Id);
        var labelText = await output.GetChildContentAsync();
        label.InnerHtml.AppendHtml(labelText);
        output.Content.AppendHtml(label);
    }
}