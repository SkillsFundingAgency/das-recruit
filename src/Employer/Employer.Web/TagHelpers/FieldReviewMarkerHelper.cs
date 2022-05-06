using System;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.ViewModels.VacancyPreview;
using Esfa.Recruit.Shared.Web.ViewModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Esfa.Recruit.Employer.Web.TagHelpers
{
    [HtmlTargetElement("div", Attributes = ValidationForAttributeName)]
    public class FieldReviewMarkerHelper : TagHelper
    {
        private const string ValidationForAttributeName = "field-review-marker";
        private const string ClassAttributeIdentifier = "class";
        private const string ErrorClassSpecifier = "app-summary-list__row--error";
        
        [HtmlAttributeName(ValidationForAttributeName)]
        public ModelExpression For { get; set; }
        
        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }
        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var model = ViewContext.ViewData.Model as VacancyPreviewViewModel;
            if (model?.Review?.FieldIndicators?.FirstOrDefault(c=>c.ReviewFieldIdentifier.Equals(For.Name, StringComparison.CurrentCultureIgnoreCase)) != null)
            {
                if (output.Attributes.ContainsName(ClassAttributeIdentifier))
                {
                    output.Attributes.TryGetAttribute(ClassAttributeIdentifier, out var classAttr);
                    output.Attributes.SetAttribute(ClassAttributeIdentifier, $"{classAttr.Value} {ErrorClassSpecifier}");
                }
                else
                {
                    output.Attributes.SetAttribute(ClassAttributeIdentifier, ErrorClassSpecifier);
                }
            }

            return Task.CompletedTask;
        }
    }
}