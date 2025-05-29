using System.IO;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.WebEncoders.Testing;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Shared.Web.TagHelpers;

public static class TagHelperOutputExtensions
{
    public static string AsString(this TagHelperOutput tagHelperOutput)
    {
        var stringWriter = new StringWriter();
        tagHelperOutput.WriteTo(stringWriter, new HtmlTestEncoder());
        return stringWriter.ToString();
    }
}