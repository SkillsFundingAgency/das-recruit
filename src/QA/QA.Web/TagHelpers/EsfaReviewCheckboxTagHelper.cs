using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Qa.Web.ViewModels;

namespace Esfa.Recruit.Qa.Web.TagHelpers
{
    using Microsoft.AspNetCore.Mvc.Rendering;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
    using Microsoft.AspNetCore.Razor.TagHelpers;

    [HtmlTargetElement(TagName)]
    public class EsfaReviewCheckboxTagHelper : TagHelper
    {
        private const string TagName = "esfa-review-checkbox";

        [HtmlAttributeName("asp-for")]
        public ModelExpression For { get; set; }

        [HtmlAttributeName("asp-items")]
        public IEnumerable<FieldIdentifierViewModel> Items { get;set; }

        [HtmlAttributeName("asp-value")]
        public string Value { get; set; }

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            /*
            <div class="govuk-form-group">
                <div class="govuk-checkboxes">
                    <div class="govuk-checkboxes__item">
                        <input type="checkbox" name="[For.Name]" id="[For.Name]-[Value]" value="Value" class="govuk-checkboxes__input field-identifier-checkbox field-identifier-checkbox">
                        <label class="govuk-label govuk-checkboxes__label" for="[For.Name]-[Value]"></label>
                    </div>
                </div>
            </div> 
            */

            var id = $"{For.Name}-{Value}";

            var input = new TagBuilder("input");
            input.Attributes.Add("type", "checkbox");
            input.Attributes.Add("id", id);
            input.Attributes.Add("name", For.Name);
            input.Attributes.Add("value", Value);
            input.AddCssClass("govuk-checkboxes__input field-identifier-checkbox");

            if(Items.Any(i => i.FieldIdentifier == Value && i.Checked))
                input.Attributes.Add("checked", "checked");

            var label = new TagBuilder("label");
            label.Attributes.Add("for", id);
            label.AddCssClass("govuk-label govuk-checkboxes__label");

            var inputParent = new TagBuilder("div");
            inputParent.AddCssClass("govuk-checkboxes__item");

            inputParent.InnerHtml.AppendHtml(input);
            inputParent.InnerHtml.AppendHtml(label);

            var inputGrandParent = new TagBuilder("div");
            inputGrandParent.AddCssClass("govuk-checkboxes");
            
            inputGrandParent.InnerHtml.AppendHtml(inputParent);

            output.TagName = "div";
            output.Attributes.Add("class", "govuk-form-group");
            output.Content.AppendHtml(inputGrandParent);

            return base.ProcessAsync(context, output);
        }
    }
}
