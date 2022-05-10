using System;
using System.Linq;
using System.Text.Encodings.Web;
using Esfa.Recruit.Employer.Web.ViewModels.VacancyPreview;
using Microsoft.AspNetCore.Mvc.TagHelpers;
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
            var error = Model.Review.FieldIndicators.FirstOrDefault(c =>
                c.ReviewFieldIdentifier.Equals(FieldName, StringComparison.CurrentCultureIgnoreCase));

            if (error == null)
            {
                return;
            }
            
            tagHelperOutput.TagName = "span";
            tagHelperOutput.AddClass("app-summary-list__error-message", HtmlEncoder.Default);
            tagHelperOutput.Content.AppendHtml(error.ManualQaText);
            foreach (string autoQaText in error.AutoQaTexts)
            {
                tagHelperOutput.Content.AppendHtml("<br/>");
                tagHelperOutput.Content.Append($"{autoQaText}");
            }
            
            tagHelperOutput.TagMode = TagMode.StartTagAndEndTag;  
        }
    }
}