using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using Esfa.Recruit.Provider.Web.ViewModels.VacancyPreview;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Esfa.Recruit.Provider.Web.TagHelpers;

[HtmlTargetElement("div", Attributes = ValidationForAttributeName)]
public class FieldReviewMarkerHelper : TagHelper
{
    private const string ValidationForAttributeName = "field-review-marker";
    private const string ErrorClassSpecifier = "app-summary-list__row--error";

    [HtmlAttributeName(ValidationForAttributeName)]
    public ModelExpression For { get; set; }

    [HtmlAttributeNotBound] [ViewContext] public ViewContext ViewContext { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var fieldName = For.Name;
        if (Lookup.TryGetValue(fieldName, out var value))
        {
            fieldName = value;
        }
            
        var model = ViewContext.ViewData.Model as VacancyPreviewViewModel;
        var isValidationError = model?.ValidationErrors?.ModelState.Any(x =>
            x.Key.Equals(fieldName, StringComparison.CurrentCultureIgnoreCase) &&
            x.Value is { Errors.Count : > 0 }) ?? false;
                
        var isReviewError =
            model?.Review?.FieldIndicators?.FirstOrDefault(c =>
                c.ReviewFieldIdentifier.Equals(fieldName, StringComparison.CurrentCultureIgnoreCase)) is not null;

        if (isValidationError || isReviewError)
        {
            output.AddClass(ErrorClassSpecifier, HtmlEncoder.Default);
        }
    }

    private static Dictionary<string, string> Lookup =>
        new()
        {
            { "WorkingWeekDescription", "WorkingWeek" },
            { "HoursPerWeek", "WorkingWeek" },
            { "WageText", "Wage" },
            { "EmployerAddressElements", "EmployerAddress" },
            { "AvailableLocations", "EmployerAddress" },
            { "IsDisabilityConfident", "DisabilityConfident" },
            { "TrainingTitle", "Training" },
            { "ProviderContactName", "ProviderContact" },
            { "ProviderContactEmail", "ProviderContact" },
            { "ProviderContactTelephone", "ProviderContact" },
            { "RouteTitle", "TraineeRoute" }
        };
}