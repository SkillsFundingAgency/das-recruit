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

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            /*
            <div class="form-group">
                <div class="multiple-choice">
                    <input type="checkbox" id="[For.Name]" name="[For.Name]" value="true">
                    <label for="[For.Name]"></label>
                </div>
            </div> 
            */

            var input = new TagBuilder("input");
            input.Attributes.Add("type", "checkbox");
            input.Attributes.Add("id", For.Name);
            input.Attributes.Add("name", For.Name);
            input.Attributes.Add("value", "true");

            if((bool)For.Model)
                input.Attributes.Add("checked", "checked");

            var label = new TagBuilder("label");
            label.Attributes.Add("for", For.Name);

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
