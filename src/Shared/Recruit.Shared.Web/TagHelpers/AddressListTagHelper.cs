using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Esfa.Recruit.Shared.Web.TagHelpers;

[HtmlTargetElement(TagName, TagStructure = TagStructure.NormalOrSelfClosing)]
public class AddressListTagHelper: TagHelper
{
    public const string TagName = "address-list";
    public bool Abridged { get; set; } = false;
    public bool Anonymised { get; set; } = false;
    public IEnumerable<Address> Addresses { get; set; }
    public string Class { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (Addresses == null)
        {
            output.SuppressOutput();
            return;
        }

        output.TagName = "ul";
        output.TagMode = TagMode.StartTagAndEndTag;

        AddCssClasses(output, Class);
        Render(output, Addresses.ToList(), Anonymised, Abridged);
    }

    private static void Render(TagHelperOutput output, List<Address> addresses, bool anonymised, bool abridged)
    {
        var locations = anonymised switch
        {
            false when !abridged => addresses.OrderByCity().Select(x => x.ToSingleLineFullAddress()),
            false => addresses.OrderByCity().Select(x => x.ToSingleLineAbridgedAddress()),
            _ => addresses.Select(x => x.ToSingleLineAnonymousAddress()).Order().Distinct()
        };

        foreach (string location in locations)
        {
            var li = new TagBuilder("li");
            li.InnerHtml.AppendHtml(location);
            output.Content.AppendHtml(li);
        }
    }

    private static void AddCssClasses(TagHelperOutput output, string cssClasses)
    {
        string[] classes = cssClasses?.Split(' ').Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
        foreach (string cssClass in classes ?? [])
        {
            output.AddClass(cssClass, HtmlEncoder.Default);
        }
    }
}