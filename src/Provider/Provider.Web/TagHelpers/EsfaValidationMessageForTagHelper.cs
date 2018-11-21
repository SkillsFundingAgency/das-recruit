using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Esfa.Recruit.Provider.Web.TagHelpers
{
    [HtmlTargetElement("span", Attributes = ValidationForAttributeName)]
    public class EsfaValidationMessageForTagHelper : ValidationMessageTagHelper
    {
        private const string ValidationForAttributeName = "esfa-validation-message-for";

        [HtmlAttributeName(ValidationForAttributeName)]
        public ModelExpression ForField { get; set; }

        public EsfaValidationMessageForTagHelper(IHtmlGenerator generator) : base(generator) { }

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            base.For = ForField;
            output.Attributes.SetAttribute("id", "error-message-" + For.Name);
            return base.ProcessAsync(context, output);
        }
    }
}