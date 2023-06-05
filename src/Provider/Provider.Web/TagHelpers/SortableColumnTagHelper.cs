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

namespace Esfa.Recruit.Provider.Web.TagHelpers
{
    [HtmlTargetElement("sortable-column")]
    public class SortableColumnTagHelper : TagHelper
    {
        private const string CssClass = "govuk-link das-table__sort";

        [HtmlAttributeName("column-name")]
        public SortColumn ColumnName { get; set; }

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

            var sortColumn = GetSortColumnFromQueryString();
            var sortOrder = GetSortOrderFromQueryString();
            var isSortColumn = sortColumn == ColumnName || (sortColumn == SortColumn.Default && IsDefault);

            var values = new
            {
                SearchTerm = GetSearchTermFromQueryString(),
                SortColumn = ColumnName,
                SortOrder = isSortColumn ? sortOrder.Reverse().ToString() : DefaultSortOrder.ToString()
            };

            var href = _urlHelper.Action(action, controller, values, null, null, null);

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
            if (ViewContext.HttpContext.Request.Query.TryGetValue("SortOrder", out var sortOrderValue) && Enum.TryParse<SortOrder>(sortOrderValue, true, out var parsedSortOrder))
            {
                return parsedSortOrder;
            }

            return DefaultSortOrder;
        }

        private SortColumn GetSortColumnFromQueryString()
        {
            if (ViewContext.HttpContext.Request.Query.TryGetValue("SortColumn", out var sortColumn) && Enum.TryParse<SortColumn>(sortColumn, true, out var parsedSortColumn))
            {
                return parsedSortColumn;
            }

            return SortColumn.Default;
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

