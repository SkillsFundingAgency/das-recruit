using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Esfa.Recruit.Shared.Web.TagHelpers
{
    [HtmlTargetElement("span", Attributes = ValidationForAttributeName)]
    public class EsfaValidationMessageForTagHelper : ValidationMessageTagHelper
    {
        private const string ValidationForAttributeName = "esfa-validation-message-for";

        [HtmlAttributeName(ValidationForAttributeName)]
        public ModelExpression ForField { get; set; }

        [HtmlAttributeName("show-all")]
        public bool ShowAll { get; set; } = false;

        public EsfaValidationMessageForTagHelper(IHtmlGenerator generator) : base(generator) { }

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            base.For = ForField;
            output.Attributes.SetAttribute("id", "error-message-" + For.Name);

            if (!ShowAll)
            {
                return base.ProcessAsync(context, output);
            }

            var entry = ViewContext?.ModelState[For.Name];

            if (entry != null && entry.Errors.Any())
            {
                output.TagName = "span";
                output.Attributes.SetAttribute("class", "govuk-error-message");

                var messages = string.Join("<br/>",
                                           entry.Errors.Select(e =>
                                               HtmlEncoder.Default.Encode(e.ErrorMessage)));

                output.Content.SetHtmlContent(messages);
            }
            return base.ProcessAsync(context, output);
        }
    }
}