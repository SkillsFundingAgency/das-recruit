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
            <div class="form-group">
                <div class="multiple-choice">
                    <input type="checkbox" name="[For.Name]" id="[For.Name]-[Value]" value="Value" class="field-identifer-checkbox">
                    <label for="[For.Name]-[Value]"></label>
                </div>
            </div> 
            */

            var id = $"{For.Name}-{Value}";

            var input = new TagBuilder("input");
            input.Attributes.Add("type", "checkbox");
            input.Attributes.Add("id", id);
            input.Attributes.Add("name", For.Name);
            input.Attributes.Add("value", Value);
            input.AddCssClass("field-identifer-checkbox");

            if(Items.Any(i => i.FieldIdentifier == Value && i.Checked))
                input.Attributes.Add("checked", "checked");

            var label = new TagBuilder("label");
            label.Attributes.Add("for", id);

            var inputParent = new TagBuilder("div");
            inputParent.AddCssClass("multiple-choice");

            inputParent.InnerHtml.AppendHtml(input);
            inputParent.InnerHtml.AppendHtml(label);
            
            output.TagName = "div";
            output.Attributes.Add("class", "form-group");
            output.Content.AppendHtml(inputParent);

            return base.ProcessAsync(context, output);
        }
    }
}
