using System.Linq;
using System.Text.Encodings.Web;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Esfa.Recruit.Shared.Web.TagHelpers;

[HtmlTargetElement("address", TagStructure = TagStructure.NormalOrSelfClosing)]
public class AddressTagHelper: TagHelper
{
    public const string TagName = "p";
    public bool Anonymised { get; set; } = false;
    public Address Value { get; set; }
    public string Class { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = TagName;
        output.TagMode = TagMode.StartTagAndEndTag;
        
        if (Value == null)
        {
            output.SuppressOutput();
            return;
        }

        string[] classes = Class?.Split(' ').Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
        foreach (string cssClass in classes ?? [])
        {
            output.AddClass(cssClass, HtmlEncoder.Default);
        }

        if (Anonymised)
        {
            Render(output, Value.PostcodeAsOutcode(), false);
            return;
        }
        
        RenderFullAddress(output, Value);
    }

    private static void RenderFullAddress(TagHelperOutput output, Address address)
    {
        Render(output, address.AddressLine1);
        Render(output, address.AddressLine2);
        Render(output, address.AddressLine3);
        Render(output, address.AddressLine4);
        Render(output, address.Postcode, false);
    }
    
    private static void Render(TagHelperOutput output, string text, bool crlf = true)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        string br = crlf ? "<br>" : string.Empty;
        output.Content.AppendHtml($"{text}{br}");
    }
}