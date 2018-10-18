using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Esfa.Recruit.Shared.Web.TagHelpers
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

    [HtmlTargetElement(Attributes = "asp-hide")]
    public class HideTagHelper : TagHelper
    {
        public bool AspHide { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (AspHide)
            {
                output.SuppressOutput();
            }
        }
    }

    [HtmlTargetElement(Attributes = "asp-show")]
    public class ShowTagHelper : TagHelper
    {
        public bool AspShow { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (!AspShow)
            {
                output.SuppressOutput();
            }
        }
    }
}