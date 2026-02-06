using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Esfa.Recruit.Shared.Web.TagHelpers;

[HtmlTargetElement("input", Attributes = $"{AspForAttributeName},{ErrorClassAttributeName}")]
public class InputErrorTagHelper : TagHelper
{
    private const string AspForAttributeName = "asp-for";
    private const string ErrorClassAttributeName = "handle-errors-css";

    [HtmlAttributeName(AspForAttributeName)]
    public ModelExpression ForField { get; set; }

    [HtmlAttributeName(ErrorClassAttributeName)]
    public bool HandleErrors { get; set; } = true;
    
    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        if (HandleErrors
            && ViewContext.ModelState.TryGetValue(ForField.Name, out var modelStateEntry) 
            && modelStateEntry.ValidationState == ModelValidationState.Invalid)
        {
            output.AddClass("govuk-input--error", HtmlEncoder.Default);
        }
            
        return base.ProcessAsync(context, output);
    }
}