using System.Text.Encodings.Web;
using Esfa.Recruit.Provider.Web.ViewModels.VacancyPreview;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Esfa.Recruit.Provider.Web.TagHelpers
{
    [HtmlTargetElement("review-field")]
    public class ReviewTagHelper : TagHelper
    {
        private readonly IFieldReviewHelper _fieldReviewHelper;

        public ReviewTagHelper(IFieldReviewHelper fieldReviewHelper)
        {
            _fieldReviewHelper = fieldReviewHelper;
        }
        public string Class { get; set; }
        public string FieldName { get; set; }
        public string TagName { get; set; }
        public VacancyPreviewViewModel Model { get; set; }
        
        public override void Process(TagHelperContext tagHelperContext, TagHelperOutput tagHelperOutput)
        {
            if (_fieldReviewHelper.ShowReviewField(Model, FieldName))
            {
                tagHelperOutput.TagName = TagName;
                tagHelperOutput.AddClass("govuk-tag",HtmlEncoder.Default);
                tagHelperOutput.AddClass(Class,HtmlEncoder.Default);
                tagHelperOutput.Content.SetHtmlContent("AMENDED BY EMPLOYER");
                tagHelperOutput.TagMode = TagMode.StartTagAndEndTag;    
            }
        }
    }
}