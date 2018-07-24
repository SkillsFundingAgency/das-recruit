using Esfa.Recruit.Shared.Web.FeatureToggle;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Esfa.Recruit.Shared.Web.TagHelpers
{
    [HtmlTargetElement(TagName)]
    public class EsfaFeatureTagHelper : TagHelper
    {
        private const string TagName = "esfaFeature";
        private readonly IFeature _feature;

        [HtmlAttributeName("name")]
        public string Name { get; set; }

        public EsfaFeatureTagHelper(IFeature feature)
        {
            _feature = feature;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (!_feature.IsFeatureEnabled(Name))
                output.SuppressOutput();
        }
    }
}
