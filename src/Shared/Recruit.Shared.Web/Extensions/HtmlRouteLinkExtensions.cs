using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace Esfa.Recruit.Shared.Web.Extensions
{
    public static class HtmlRouteLinkExtensions
    {
        public static IHtmlContent RouteLink(IUrlHelper urlHelper, string linkText, string routeName, object routeValues, object htmlAttributes,string tag = "span")
        {
            // create wrapper around text e.g. <span>linkText</span>
            IList<IDictionary<string, string>> list=new List<IDictionary<string, string>>();
            var tagBuilderInner = new TagBuilder(tag);
            tagBuilderInner.AddCssClass("govuk-visually-hidden");
            tagBuilderInner.InnerHtml.Append((!String.IsNullOrEmpty(linkText)) ? HttpUtility.HtmlEncode(linkText) : String.Empty);

            // create wrapper around wrapper around text e.g. <a><span>linkText</span></a>
            var tagBuilderAtag = new TagBuilder("a");
            tagBuilderAtag.AddCssClass("govuk-link das-edit-link");
            tagBuilderAtag.InnerHtml.Append(linkText);
            tagBuilderAtag.InnerHtml.AppendHtml(tagBuilderInner);
            // add the href to the a element
            tagBuilderAtag.MergeAttribute("href", urlHelper.RouteUrl(routeValues));
            //tagBuilderAtag.MergeAttribute("data-automation",htmlAttributes["data-automation"].]);

            return tagBuilderAtag;
        }
    }
}
