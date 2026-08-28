using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Esfa.Recruit.Shared.Web.TagHelpers;

[HtmlTargetElement(TagName)]
[OutputElementHint("th")]
public class ButtonSortableColumnHeaderTagHelper : TagHelper
{
    public const string TagName = "button-sortable-column-header";
    
    public string ActiveSortColumn { get; set; }
    public string SortColumn { get; set; }
    
    public ColumnSortOrder DefaultSortOrder { get; set; }
    public ColumnSortOrder? ActiveSortOrder { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "th";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.Add("aria-sort", GetCurrentAriaSortOrder(ActiveSortOrder));
        output.AddClass("govuk-table__header", HtmlEncoder.Default);
        output.Attributes.Add("scope", "col");

        var nextSortOrder = GetNextSortOrder(ActiveSortOrder);

        var button = new TagBuilder("button");
        button.Attributes.Add("data-sort-column", SortColumn);
        button.Attributes.Add("data-sort-order", nextSortOrder.ToString());
        
        button.InnerHtml.AppendHtml(await output.GetChildContentAsync());
        button.InnerHtml.AppendHtml(GetSortIcon(ActiveSortOrder));

        output.Content.AppendHtml(button);
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
        if (SortColumn != ActiveSortColumn)
        {
            return DefaultSortOrder;
        }
        
        return sortOrder switch
        {
            ColumnSortOrder.Asc => ColumnSortOrder.Desc,
            _ => ColumnSortOrder.Asc,
        };
    }
    
    private string GetCurrentAriaSortOrder(ColumnSortOrder? sortOrder)
    {
        if (SortColumn != ActiveSortColumn)
        {
            return "none";
        }

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