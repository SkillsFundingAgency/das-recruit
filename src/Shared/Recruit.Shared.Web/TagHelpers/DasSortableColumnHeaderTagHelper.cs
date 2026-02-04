using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Shared.Web.TagHelpers;

[HtmlTargetElement(TagName)]
[OutputElementHint("th")]
public class DasSortableColumnHeaderTagHelper(IUrlHelperFactory urlHelperFactory, IActionContextAccessor contextAccessor) : TagHelper
{
    public const string TagName = "das-sortable-column-header";
    
    public string ActiveSortColumn { get; set; }
    public string SortColumn { get; set; }
    
    public ColumnSortOrder DefaultSortOrder { get; set; }
    public ColumnSortOrder? ActiveSortOrder { get; set; }

    [HtmlAttributeName("route-data")]
    public Dictionary<string, string> RouteDictionary { get; set; }
    
    [HtmlAttributeNotBound, ViewContext]
    public ViewContext ViewContext { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "th";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.AddClass("govuk-table__header", HtmlEncoder.Default);
        output.Attributes.Add("scope", "col");

        var action = ViewContext.RouteData.Values["action"] as string;
        var controller = ViewContext.RouteData.Values["controller"] as string;
        var urlHelper = urlHelperFactory.GetUrlHelper(contextAccessor.ActionContext!);
        var nextSortOrder = GetNextSortOrder(ActiveSortOrder);

        var anchor = new TagBuilder("a");
        anchor.AddCssClass("govuk-link");
        anchor.AddCssClass("govuk-link--no-visited-state");
        anchor.AddCssClass("das-sortable-column-header");
        anchor.Attributes.Add("aria-sort", GetCurrentAriaSortOrder(ActiveSortOrder));
        anchor.InnerHtml.AppendHtml(await output.GetChildContentAsync());
        anchor.InnerHtml.AppendHtml(GetSortIcon(nextSortOrder));

        var routeDictionary = new Dictionary<string, string>(RouteDictionary)
        {
            ["sortColumn"] = SortColumn,
            ["sortOrder"] = $"{nextSortOrder}"
        };
        anchor.Attributes.Add("href", urlHelper.Action(action, controller, routeDictionary));
        output.Content.AppendHtml(anchor);
    }

    private TagBuilder GetSortIcon(ColumnSortOrder? nextSortOrder)
    {
        if (ActiveSortColumn != SortColumn)
        {
            return SortIcon;
        }
        
        return nextSortOrder switch
        {
            ColumnSortOrder.Asc => AscIcon,
            _ => DescIcon,
        };
    }
    
    private ColumnSortOrder GetNextSortOrder(ColumnSortOrder? sortOrder)
    {
        return sortOrder switch
        {
            ColumnSortOrder.Asc => ColumnSortOrder.Desc,
            ColumnSortOrder.Desc => ColumnSortOrder.Asc,
            _ => DefaultSortOrder,
        };
    }
    
    private static string GetCurrentAriaSortOrder(ColumnSortOrder? sortOrder)
    {
        return sortOrder switch
        {
            ColumnSortOrder.Asc => "ascending",
            ColumnSortOrder.Desc => "descending",
            _ => "none",
        };
    }

    private static TagBuilder SortIcon { get; } = CreateSortIcon(); 
    private static TagBuilder AscIcon { get; } = CreateAscIcon(); 
    private static TagBuilder DescIcon { get; } = CreateDescIcon(); 

    private static TagBuilder CreateSortIcon()
    {
        var svg = new TagBuilder("svg");
        svg.Attributes.Add("width", "22");
        svg.Attributes.Add("height", "22");
        svg.Attributes.Add("focusable", "false");
        svg.Attributes.Add("aria-hidden", "true");
        svg.Attributes.Add("role", "img");
        svg.Attributes.Add("viewBox", "0 0 22 22");
        svg.Attributes.Add("fill", "none");
        svg.Attributes.Add("xmlns", "http://www.w3.org/2000/svg");

        var upPath = new TagBuilder("path");
        upPath.Attributes.Add("d", "M8.1875 9.5L10.9609 3.95703L13.7344 9.5H8.1875Z");
        upPath.Attributes.Add("fill", "currentColor");
        
        var downPath = new TagBuilder("path");
        downPath.Attributes.Add("d", "M13.7344 12.0781L10.9609 17.6211L8.1875 12.0781H13.7344Z");
        downPath.Attributes.Add("fill", "currentColor");

        svg.InnerHtml.AppendHtml(upPath);
        svg.InnerHtml.AppendHtml(downPath);

        return svg;
    }
    
    private static TagBuilder CreateAscIcon()
    {
        var svg = new TagBuilder("svg");
        svg.Attributes.Add("width", "22");
        svg.Attributes.Add("height", "22");
        svg.Attributes.Add("focusable", "false");
        svg.Attributes.Add("aria-hidden", "true");
        svg.Attributes.Add("role", "img");
        svg.Attributes.Add("viewBox", "0 0 22 22");
        svg.Attributes.Add("fill", "none");
        svg.Attributes.Add("xmlns", "http://www.w3.org/2000/svg");

        var upPath = new TagBuilder("path");
        upPath.Attributes.Add("d", "M6.5625 15.5L11 6.63125L15.4375 15.5H6.5625Z");
        upPath.Attributes.Add("fill", "currentColor");

        svg.InnerHtml.AppendHtml(upPath);

        return svg;
    }
    
    private static TagBuilder CreateDescIcon()
    {
        var svg = new TagBuilder("svg");
        svg.Attributes.Add("width", "22");
        svg.Attributes.Add("height", "22");
        svg.Attributes.Add("focusable", "false");
        svg.Attributes.Add("aria-hidden", "true");
        svg.Attributes.Add("role", "img");
        svg.Attributes.Add("viewBox", "0 0 22 22");
        svg.Attributes.Add("fill", "none");
        svg.Attributes.Add("xmlns", "http://www.w3.org/2000/svg");
        
        var downPath = new TagBuilder("path");
        downPath.Attributes.Add("d", "M15.4375 7L11 15.8687L6.5625 7L15.4375 7Z");
        downPath.Attributes.Add("fill", "currentColor");

        svg.InnerHtml.AppendHtml(downPath);

        return svg;
    }
}