using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Esfa.Recruit.Employer.Web.TagHelpers
{
    [HtmlTargetElement("div", Attributes = ValidationForAttributeName)]
    public class FieldReviewMarkerHelper : TagHelper
    {
        private const string ValidationForAttributeName = "field-review-marker";
        private const string ClassAttributeIdentifier = "class";
        private const string ErrorClassSpecifier = "app-summary-list__row--error";
        
        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        [HtmlAttributeName(ValidationForAttributeName)]
        public ModelExpression For { get; set; }
        
        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (ViewContext.ModelState.TryGetValue(For.Name, out var modelStateEntry) && modelStateEntry.ValidationState == ModelValidationState.Invalid)
            {
                if (output.Attributes.ContainsName(ClassAttributeIdentifier))
                {
                    output.Attributes.TryGetAttribute(ClassAttributeIdentifier, out var classAttr);
                    output.Attributes.SetAttribute(ClassAttributeIdentifier, $"{classAttr.Value} {ErrorClassSpecifier}");
                }
                else
                {
                    output.Attributes.SetAttribute(ClassAttributeIdentifier, ErrorClassSpecifier);
                }
            }

            return Task.CompletedTask;
        }
    }
}