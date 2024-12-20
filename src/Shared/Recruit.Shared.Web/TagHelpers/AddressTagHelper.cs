using System.Linq;
using System.Text.Encodings.Web;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Esfa.Recruit.Shared.Web.TagHelpers;

[HtmlTargetElement(TagName, TagStructure = TagStructure.NormalOrSelfClosing)]
public class AddressTagHelper: TagHelper
{
    public const string TagName = "address";
    public bool Anonymised { get; set; } = false;
    public Address Value { get; set; }
    public bool Flat { get; set; } = false;
    public string Class { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (Value == null)
        {
            output.SuppressOutput();
            return;
        }
        
        output.TagName = Flat ? "span" : "p";
        output.TagMode = TagMode.StartTagAndEndTag;

        AddCssClasses(output, Class);
        Render(output, Value, Anonymised, Flat);
    }

    private static void Render(TagHelperOutput output, Address address, bool anonymised, bool flat)
    {
        // outcode only
        if (anonymised)
        {
            output.Content.AppendHtml(address.PostcodeAsOutcode());
            return;
        }
        
        string[] lines = new[]
        {
            address.AddressLine1,
            address.AddressLine2,
            address.AddressLine3,
            address.AddressLine4,
            address.Postcode
        }.Where(x => !string.IsNullOrEmpty(x?.Trim())).ToArray();

        // single line
        if (flat)
        {
            output.Content.AppendHtml(string.Join(", ", lines));
            return;
        }
        
        // multi line
        int index = 0;
        foreach (string line in lines)
        {
            string br = ++index < lines.Length ? "<br>" : string.Empty;
            output.Content.AppendHtml($"{line}{br}");
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