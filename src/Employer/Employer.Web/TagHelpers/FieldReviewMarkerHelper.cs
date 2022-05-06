using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.ViewModels.VacancyPreview;
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

        [HtmlAttributeNotBound] [ViewContext] public ViewContext ViewContext { get; set; }

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var fieldName = For.Name;

            if (Lookup.ContainsKey(fieldName))
            {
                fieldName = Lookup[fieldName];
            }
            
            var model = ViewContext.ViewData.Model as VacancyPreviewViewModel;
            if (model?.Review?.FieldIndicators?.FirstOrDefault(c =>
                    c.ReviewFieldIdentifier.Equals(fieldName, StringComparison.CurrentCultureIgnoreCase)) != null)
            {
                if (output.Attributes.ContainsName(ClassAttributeIdentifier))
                {
                    output.Attributes.TryGetAttribute(ClassAttributeIdentifier, out var classAttr);
                    output.Attributes.SetAttribute(ClassAttributeIdentifier,
                        $"{classAttr.Value} {ErrorClassSpecifier}");
                }
                else
                {
                    output.Attributes.SetAttribute(ClassAttributeIdentifier, ErrorClassSpecifier);
                }
            }

            return Task.CompletedTask;
        }

        private static Dictionary<string, string> Lookup
        {
            get
            {
                return new Dictionary<string, string>
                {
                    { "WorkingWeekDescription", "WorkingWeek" },
                    { "HoursPerWeek", "WorkingWeek" },
                    { "WageText", "Wage" },
                    { "EmployerAddressElements", "EmployerAddress" },
                    { "IsDisabilityConfident", "DisabilityConfident" },
                    { "TrainingTitle", "Training" },
                };
            }
        }
    }
}