using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text.Encodings.Web;

namespace Esfa.Recruit.Shared.Web.TagHelpers
{
    [HtmlTargetElement("div", Attributes = HighlightErrorForAttributeName + "," + ErrorCssClass)]
    [HtmlTargetElement("textarea", Attributes = HighlightErrorForAttributeName + "," + ErrorCssClass)]
    [HtmlTargetElement("input", Attributes = HighlightErrorForAttributeName + "," + ErrorCssClass)]
    public class DasHighlightErrorsTagHelper : TagHelper
    {
        private const string HighlightErrorForAttributeName = "das-highlight-error-for";
        private const string ErrorCssClass = "error-class";

        [HtmlAttributeName(HighlightErrorForAttributeName)]
        public ModelExpression Property { get; set; }

        [HtmlAttributeName(ErrorCssClass)]
        public string CssClass { get; set; }

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }


        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (!ViewContext.ModelState.ContainsKey(Property.Name)) return;

            var modelState = ViewContext.ModelState[Property.Name];

            if (modelState.Errors.Count == 0) return;
            output.AddClass(CssClass, HtmlEncoder.Default);
        }
    }
}
