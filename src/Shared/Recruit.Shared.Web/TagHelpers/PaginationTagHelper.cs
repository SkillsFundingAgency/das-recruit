using Esfa.Recruit.Shared.Web.ViewModels;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Esfa.Recruit.Shared.Web.TagHelpers;

[HtmlTargetElement(TagName)]
[OutputElementHint("div")]
public class PaginationTagHelper(IUrlHelperFactory urlHelperFactory, IActionContextAccessor contextAccessor) : TagHelper
{
    public const string TagName = "das-pagination";

    public PagerViewModel Model { get; set; }
    
    [HtmlAttributeNotBound, ViewContext]
    public ViewContext ViewContext { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "nav";
        output.TagMode = TagMode.StartTagAndEndTag;
        
        output.Attributes.SetAttribute("role", "navigation");
        output.Attributes.SetAttribute("aria-label", "Pagination");
        
        output.Content.AppendHtml(GetCaption());
        output.Content.AppendHtml(GetPagination());
    }

    private IHtmlContent GetPagination()
    {
        var action = ViewContext.RouteData.Values["action"] as string;
        var controller = ViewContext.RouteData.Values["controller"] as string;
        var urlHelper = urlHelperFactory.GetUrlHelper(contextAccessor.ActionContext!);
        
        var ul = new TagBuilder("ul");
        ul.AddCssClass("das-pagination");

        if (Model.ShowPrevious)
        {
            var a = new TagBuilder("a");
            a.AddCssClass("das-pagination__link");
            a.Attributes.Add("aria-label", "Previous page");
            a.Attributes.Add("href", urlHelper.Action(action, controller, Model.PreviousPageRouteData));
            a.InnerHtml.Append("Previous");
            
            ul.InnerHtml.AppendHtml(GetPageItem(a));
        }

        for (var pageIndex = 1; pageIndex <= Model.TotalPages; pageIndex++)
        {
            var a = new TagBuilder("a");
            a.AddCssClass("das-pagination__link");

            if (pageIndex == Model.CurrentPage)
            {
                a.AddCssClass("current");
                a.Attributes.Add("aria-current", "true");
                a.Attributes.Add("aria-label", $"Page {pageIndex}, current page");
            }
            else
            {
                a.Attributes.Add("aria-label", $"Page {pageIndex}");
            }
            
            a.Attributes.Add("href", urlHelper.Action(action, controller, Model.GetRouteData(pageIndex)));
            a.InnerHtml.Append($"{pageIndex}");
            
            ul.InnerHtml.AppendHtml(GetPageItem(a));
        }
        
        if (Model.ShowNext)
        {
            var a = new TagBuilder("a");
            a.AddCssClass("das-pagination__link");
            a.Attributes.Add("aria-label", "Next page");
            a.Attributes.Add("href", urlHelper.Action(action, controller, Model.NextPageRouteData));
            a.InnerHtml.Append("Next");
            
            ul.InnerHtml.AppendHtml(GetPageItem(a));
        }

        return ul;
    }
    
    private static IHtmlContent GetPageItem(IHtmlContent anchor)
    {
        var li = new TagBuilder("li");
        li.AddCssClass("das-pagination__item");
        li.InnerHtml.AppendHtml(anchor);
        return li;
    }

    private IHtmlContent GetCaption()
    {
        var div = new TagBuilder("div");
        div.AddCssClass("das-pagination__summary");
        div.InnerHtml.Append(Model.Caption);
        return div;
    }
}