using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Esfa.Recruit.Shared.Web.TagHelpers;

[HtmlTargetElement(TagName)]
public class EsfaFeatureEnabledTagHelper(IFeature feature) : TagHelper
{
    private const string TagName = "esfaFeatureEnabled";

    [HtmlAttributeName("name")]
    public string Name { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (feature.IsFeatureEnabled(Name))
        {
            output.TagName = null;
            return;
        }
            
        output.SuppressOutput();
    }
}

[HtmlTargetElement(TagName)]
public class EsfaFeatureDisabledTagHelper(IFeature feature) : TagHelper
{
    private const string TagName = "esfaFeatureDisabled";

    [HtmlAttributeName("name")]
    public string Name { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (!feature.IsFeatureEnabled(Name))
        {
            output.TagName = null;
            return;
        }
            
        output.SuppressOutput();
    }
}