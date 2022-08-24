using System;
using System.Linq;
using System.Text.Encodings.Web;
using Esfa.Recruit.Provider.Web.ViewModels.VacancyPreview;
using Esfa.Recruit.Shared.Web.ViewModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Esfa.Recruit.Provider.Web.TagHelpers
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


            var validationError = string.Empty;
            if (Model.ValidationErrors.ModelState != null 
                && Model.ValidationErrors.ModelState.ContainsKey(FieldName) 
                && Model.ValidationErrors.ModelState[FieldName].ValidationState == ModelValidationState.Invalid)
            {
                var item = Model.ValidationErrors.ModelState[FieldName];
                validationError = item.Errors.FirstOrDefault()?.ErrorMessage;
            }
            
            if (error == null && string.IsNullOrEmpty(validationError))
            {
                return;
            }
            
            tagHelperOutput.TagName = "span";
            tagHelperOutput.AddClass("app-summary-list__error-message", HtmlEncoder.Default);
            if (!string.IsNullOrEmpty(validationError))
            {
                tagHelperOutput.Content.AppendHtml(validationError);
                if (error != null)
                {
                    tagHelperOutput.Content.AppendHtml("<br/>");
                }
            }

            if (error != null)
            {
                tagHelperOutput.Content.AppendHtml(error.ManualQaText);
                foreach (string autoQaText in error.AutoQaTexts)
                {
                    tagHelperOutput.Content.AppendHtml("<br/>");
                    tagHelperOutput.Content.Append($"{autoQaText}");
                }    
            }
            
            
            tagHelperOutput.TagMode = TagMode.StartTagAndEndTag;  
        }
    }
}