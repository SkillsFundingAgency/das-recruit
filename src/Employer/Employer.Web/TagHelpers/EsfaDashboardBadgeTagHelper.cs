using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.TagHelpers
{
    [HtmlTargetElement(TagName)]
    public class EsfaDashboardBadgeTagHelper : AnchorTagHelper
    {
        private const string TagName = "esfa-dashboard-badge";
        private const string DefaultNonAnchorTagName = "span";
        public EsfaDashboardBadgeTagHelper(IHtmlGenerator generator) : base(generator) { }

        [HtmlAttributeName("asp-for")]
        public ModelExpression For { get; set; }

        [HtmlAttributeName("asp-route-statusFilter")]
        public string TagStatusFilter { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (For.Model == null && string.IsNullOrEmpty(TagStatusFilter)) // All (empty statusFilter)
            {
                output.TagName = DefaultNonAnchorTagName;
            }
            else if (((VacancyStatus?)For.Model).HasValue && ((VacancyStatus)For.Model).ToString().Equals(TagStatusFilter)) // Currently applied filter
            {
                output.TagName = DefaultNonAnchorTagName;
            }
            else
            {
                var link = Generator.GenerateRouteLink(
                    viewContext: ViewContext,
                    linkText: string.Empty,
                    routeName: Route,
                    protocol: Protocol,
                    hostName: Host,
                    fragment: Fragment,
                    routeValues: new { statusFilter = TagStatusFilter },
                    htmlAttributes: new { @class = output.Attributes["class"]?.Value });
                var builder = await GetAnchorBadgeHtmlBuilder(output, link);
                output.Content.SetHtmlContent(builder);
            }
            await base.ProcessAsync(context, output);
        }

        private static async Task<HtmlContentBuilder> GetAnchorBadgeHtmlBuilder(TagHelperOutput output, TagBuilder link)
        {
            var builder = new HtmlContentBuilder();
            builder.AppendHtml(link.RenderStartTag());
            builder.AppendHtml(await output.GetChildContentAsync());
            builder.AppendHtml(link.RenderEndTag());
            return builder;
        }
    }
}
