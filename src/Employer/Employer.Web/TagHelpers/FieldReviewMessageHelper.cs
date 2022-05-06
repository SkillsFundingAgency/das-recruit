using System.Threading.Tasks;
using System.Text.Encodings.Web;
using Esfa.Recruit.Employer.Web.ViewModels.VacancyPreview;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Esfa.Recruit.Employer.Web.TagHelpers
{
    [HtmlTargetElement("field-review-message")]
    public class FieldReviewMessageHelper : TagHelper
    {
        public string FieldName { get; set; }
        public VacancyPreviewViewModel Model { get; set; }
        public override void Process(TagHelperContext tagHelperContext, TagHelperOutput tagHelperOutput)
        {
            
            tagHelperOutput.TagName = "span";
            tagHelperOutput.AddClass("app-summary-list__error-message", HtmlEncoder.Default);
            tagHelperOutput.TagMode = TagMode.StartTagAndEndTag;  
        }
    }
}