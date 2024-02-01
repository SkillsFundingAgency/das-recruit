using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Esfa.Recruit.Shared.Web.TagHelpers
{
    [HtmlTargetElement(TagName)]
    public class EsfaFeatureEnabledTagHelper : TagHelper
    {
        private const string TagName = "esfaFeatureEnabled";
        private readonly IFeature _feature;

        [HtmlAttributeName("name")]
        public string Name { get; set; }

        public EsfaFeatureEnabledTagHelper(IFeature feature)
        {
            _feature = feature;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (_feature.IsFeatureEnabled(Name))
            {
                output.TagName = null;
                return;
            }
            
            output.SuppressOutput();
        }
    }

    [HtmlTargetElement(TagName)]
    public class EsfaFeatureDisabledTagHelper : TagHelper
    {
        private const string TagName = "esfaFeatureDisabled";
        private readonly IFeature _feature;

        [HtmlAttributeName("name")]
        public string Name { get; set; }

        public EsfaFeatureDisabledTagHelper(IFeature feature)
        {
            _feature = feature;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (_feature.IsFeatureEnabled(Name) == false)
            {
                output.TagName = null;
                return;
            }
            
            output.SuppressOutput();
        }
    }
}
