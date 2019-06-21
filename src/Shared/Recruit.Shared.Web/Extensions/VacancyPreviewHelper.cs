using System.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace Esfa.Recruit.Shared.Web.Extensions
{
    public static class VacancyPreviewHelper
    {
        public static IHtmlContent ActionLink(IUrlHelper urlHelper, string linkText, string routeName, object routeValues, string screenReaderText, string linkClass, string dataAutomationId)
        {
            // create wrapper around text e.g. <span>linkText</span>
            var tagBuilderInner = new TagBuilder("span");
            tagBuilderInner.AddCssClass("govuk-visually-hidden");
            tagBuilderInner.InnerHtml.Append(!string.IsNullOrEmpty(screenReaderText) ? HttpUtility.HtmlEncode(screenReaderText) : string.Empty);
            // create wrapper around wrapper around text e.g. <a><span>linkText</span></a>
            var tagBuilderAtag = new TagBuilder("a");
            tagBuilderAtag.AddCssClass(linkClass);
            tagBuilderAtag.InnerHtml.Append(!string.IsNullOrEmpty(linkText) ? HttpUtility.HtmlEncode(linkText) : string.Empty);
            tagBuilderAtag.InnerHtml.AppendHtml(tagBuilderInner);
            // add the other properties
            tagBuilderAtag.MergeAttribute("href", urlHelper.RouteUrl(routeName, routeValues));
            tagBuilderAtag.MergeAttribute("data-automation", dataAutomationId);
            return tagBuilderAtag;
        }
    }
}
