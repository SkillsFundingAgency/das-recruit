using System.Text.Encodings.Web;
using Esfa.Recruit.Employer.Web.ViewModels.VacancyPreview;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Esfa.Recruit.Employer.Web.TagHelpers;

[HtmlTargetElement("field-review-tag")]
public class ReviewTagHelper(IFieldReviewHelper fieldReviewHelper) : TagHelper
{
    public string Class { get; set; }
    public string FieldName { get; set; }
    public string TagName { get; set; }
    public VacancyPreviewViewModel Model { get; set; }
        
    public override void Process(TagHelperContext tagHelperContext, TagHelperOutput tagHelperOutput)
    {
        if (fieldReviewHelper.ShowReviewField(Model, FieldName))
        {
            tagHelperOutput.TagName = TagName;
            tagHelperOutput.AddClass("govuk-tag",HtmlEncoder.Default);
            tagHelperOutput.AddClass(Class,HtmlEncoder.Default);
            tagHelperOutput.Content.SetHtmlContent("Amended by provider");
            tagHelperOutput.TagMode = TagMode.StartTagAndEndTag;    
        } 
        else 
        {
            tagHelperOutput.TagName = "span";
            tagHelperOutput.TagMode = TagMode.StartTagAndEndTag;  
        }
    }
}

[HtmlTargetElement("dt-review-section")]
public class AdvisorReviewTagHelper(IFieldReviewHelper fieldReviewHelper) : TagHelper
{
    public string SectionState { get; set; }
    public override void Process(TagHelperContext context, TagHelperOutput tagHelperOutput)
    {
        tagHelperOutput.TagName = "dt";
        tagHelperOutput.AddClass("govuk-summary-list__key", HtmlEncoder.Default);
        tagHelperOutput.AddClass("app-check-answers__key", HtmlEncoder.Default);
        tagHelperOutput.AddClass(fieldReviewHelper.GetReviewSectionClass(SectionState), HtmlEncoder.Default);
        tagHelperOutput.TagMode = TagMode.StartTagAndEndTag;
    }
}