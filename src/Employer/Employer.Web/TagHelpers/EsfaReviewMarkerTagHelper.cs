using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Employer.Web.ViewModels.VacancyPreview;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Esfa.Recruit.Employer.Web.TagHelpers
{
    [HtmlTargetElement(TagName)]
    public class EsfaReviewMarkerTagHelper : TagHelper
    {
        private const string TagName = "esfa-review-marker";

        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IActionContextAccessor _actionContextAccesor;

        [HtmlAttributeName("asp-for")]
        public ModelExpression For { get; set; }

        [HtmlAttributeName("value")]
        public string Value { get; set; }

        public EsfaReviewMarkerTagHelper(IUrlHelperFactory urlHelperFactory, IActionContextAccessor actionContextAccesor)
        {
            _urlHelperFactory = urlHelperFactory;
            _actionContextAccesor = actionContextAccesor;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            /*
                <img src="~/img/red-cross.png">
            */

            var model = (IEnumerable<ReviewFieldIndicatorViewModel>) For.Model;
            
            if (model.All(r => r.ReviewFieldIdentifier != Value))
            {
                output.SuppressOutput();
                return;
            }

            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccesor.ActionContext);

            output.TagName = "img";
            output.Attributes.Add("src", urlHelper.Content("~/img/red-cross.png"));
        }
    }
}
