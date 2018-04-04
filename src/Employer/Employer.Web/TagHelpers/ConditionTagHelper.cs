using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Esfa.Recruit.Employer.Web.TagHelpers
{
    [HtmlTargetElement(Attributes = "asp-condition")]
    public class ConditionTagHelper : TagHelper
    {
        public bool AspCondition { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (!AspCondition)
            {
                output.SuppressOutput();
            }
        }
    }
}