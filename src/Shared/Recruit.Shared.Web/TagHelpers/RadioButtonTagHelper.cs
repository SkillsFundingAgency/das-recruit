using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
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

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.SuppressOutput();
        output.Content.Clear();
        var div = new TagBuilder("div");
        div.AddCssClass("govuk-radios__item");

        var dataAttributes = context.AllAttributes.Where(x => x.Name.StartsWith("data-"));
        var radioAttributes = new ExpandoObject();
        ICollection<KeyValuePair<string, object>> attributes = radioAttributes;
        attributes.Add(new KeyValuePair<string, object>("class", "govuk-radios__input"));
        attributes.Add(new KeyValuePair<string, object>("id", Id));
        foreach (var dataAttribute in dataAttributes)
        {
            attributes.Add(new KeyValuePair<string, object>(dataAttribute.Name, dataAttribute.Value));
        }

        var radio = htmlGenerator.GenerateRadioButton(
            ViewContext,
            For?.ModelExplorer,
            For?.Name,
            Value,
            null,
            radioAttributes);
        
        div.InnerHtml.AppendHtml(radio);

        var label = new TagBuilder("label");
        label.AddCssClass("govuk-label");
        label.AddCssClass("govuk-radios__label");
        label.Attributes.Add("for", Id);
        var labelText = await output.GetChildContentAsync();
        label.InnerHtml.AppendHtml(labelText);
        div.InnerHtml.AppendHtml(label);
        
        output.Content.AppendHtml(div);
    }
}