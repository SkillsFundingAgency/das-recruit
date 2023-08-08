using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Text;

namespace Esfa.Recruit.Shared.Web.TagHelpers
{
    [HtmlTargetElement("sortable-column")]
    public class SortableColumnTagHelper : TagHelper
    {
        private const string CssClass = "govuk-link das-table__sort";

        [HtmlAttributeName("column-name")]
        public string ColumnName { get; set; }

        [HtmlAttributeName("column-label")]
        public string Label { get; set; }

        [HtmlAttributeName("default")]
        public bool IsDefault { get; set; }

        [HtmlAttributeName("default-order")]
        public SortOrder DefaultSortOrder { get; set; }

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        private readonly IUrlHelper _urlHelper;

        public SortableColumnTagHelper(IUrlHelperFactory urlHelperFactory, IActionContextAccessor contextAccessor)
        {
            _urlHelper = urlHelperFactory.GetUrlHelper(contextAccessor.ActionContext);
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var action = ViewContext.RouteData.Values["action"] as string;
            var controller = ViewContext.RouteData.Values["controller"] as string;

            var sortColumn = GetColumnFromQueryString();
            var sortOrder = GetSortOrderFromQueryString();
            var isSortColumn = sortColumn == ColumnName || (string.IsNullOrWhiteSpace(sortColumn) && IsDefault);

            var values = new
            {
                SearchTerm = GetSearchTermFromQueryString(),
                SortColumn = ColumnName,
                SortOrder = isSortColumn ? sortOrder.Reverse().ToString() : DefaultSortOrder.ToString()
            };

            var href = _urlHelper.Action(action, controller, values);

            var sortOrderCssSuffix = string.Empty;
            if (isSortColumn)
            {
                sortOrderCssSuffix = sortOrder == SortOrder.Ascending ? "das-table__sort--asc" : "das-table__sort--desc";
            }

            var ariaSort = sortOrder.ToString().ToLower();

            var content = new StringBuilder();
            content.Append($"<a class=\"{CssClass} {sortOrderCssSuffix}\" href=\"{href}\" aria-sort=\"{ariaSort}\">");
            content.Append(Label);
            content.Append("</a>");

            output.TagName = "";
            output.PostContent.SetHtmlContent(content.ToString());
            output.Attributes.Clear();

        }

        private SortOrder GetSortOrderFromQueryString()
        {
            if (ViewContext.HttpContext.Request.Query.ContainsKey("SortOrder"))
            {
                if (ViewContext.HttpContext.Request.Query.TryGetValue("SortOrder", out var sortOrderValue))
                {
                    if (Enum.TryParse<SortOrder>(sortOrderValue, true, out var parsedSortOrder))
                    {
                        return parsedSortOrder;
                    }
                }
            }

            return DefaultSortOrder;
        }

        private string GetColumnFromQueryString()
        {
            if (ViewContext.HttpContext.Request.Query.ContainsKey("SortColumn"))
            {
                return ViewContext.HttpContext.Request.Query["SortColumn"];
            }

            return string.Empty;
        }

        private string GetSearchTermFromQueryString()
        {
            if (ViewContext.HttpContext.Request.Query.ContainsKey("SearchTerm"))
            {
                return ViewContext.HttpContext.Request.Query["SearchTerm"];
            }

            return string.Empty;
        }
    }
}