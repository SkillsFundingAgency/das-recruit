using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Esfa.Recruit.Shared.Web.TagHelpers;

[HtmlTargetElement("span", Attributes = ValidationForAttributeName)]
public class EsfaValidationMessageForTagHelper(IHtmlGenerator generator) : ValidationMessageTagHelper(generator)
{
    private const string ValidationForAttributeName = "esfa-validation-message-for";
    private const string ErrorPrefix = "<span class=\"govuk-visually-hidden\">Error:</span>";
    private const string ErrorMessageClass = "govuk-error-message";
    private const string ErrorMessageIdPrefix = "error-message-";

    [HtmlAttributeName(ValidationForAttributeName)]
    public ModelExpression ForField { get; set; } = default!;

    [HtmlAttributeName("show-all")]
    public bool ShowAll { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        For = ForField;

        var fieldName = For.Name;
        output.Attributes.SetAttribute("id", ErrorMessageIdPrefix + fieldName);

        var entry = ViewContext?.ModelState[fieldName];
        var hasErrors = entry?.Errors.Count > 0;

        if (!hasErrors)
        {
            await base.ProcessAsync(context, output);
            return;
        }

        if (ShowAll)
        {
            output.TagName = "span";
            output.Attributes.SetAttribute("class", ErrorMessageClass);

            var messages = string.Join(
                "<br/>",
                entry!.Errors.Select(e => HtmlEncoder.Default.Encode(e.ErrorMessage))
            );

            output.Content.SetHtmlContent(ErrorPrefix + messages);
            return;
        }

        await base.ProcessAsync(context, output);
        output.PreContent.SetHtmlContent(ErrorPrefix);
    }
}