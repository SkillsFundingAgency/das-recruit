using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace Esfa.Recruit.Shared.Web.Extensions
{
    public static class VacancyPreviewHelper
    {
        public static IHtmlContent ActionLink(IUrlHelper urlHelper, string linkText, string routeName, object routeValues, string screenReaderText, string linkClass, string dataAutomationId)
        {
            /*
             *<a href="duration?edit=yes" class="govuk-link das-edit-link">Change
             *  <span class="govuk-visually-hidden"> working week</span>
             * </a>
             */
            var visuallyHiddenSpan = new TagBuilder("span");
            visuallyHiddenSpan.AddCssClass("govuk-visually-hidden");
            visuallyHiddenSpan.InnerHtml.Append(screenReaderText);
            var tagBuilderAtag = new TagBuilder("a");
            tagBuilderAtag.AddCssClass(linkClass);
            tagBuilderAtag.InnerHtml.Append(linkText);
            tagBuilderAtag.InnerHtml.AppendHtml(visuallyHiddenSpan);
            tagBuilderAtag.MergeAttribute("href", urlHelper.RouteUrl(routeName, routeValues));
            tagBuilderAtag.MergeAttribute("data-automation", dataAutomationId);
            return tagBuilderAtag;
        }
    }
}
