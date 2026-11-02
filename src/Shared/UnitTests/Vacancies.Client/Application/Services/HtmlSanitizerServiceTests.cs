using Esfa.Recruit.Vacancies.Client.Application.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Esfa.Recruit.UnitTests.Vacancies.Client.Application.Services;

public class HtmlSanitizerServiceTests
{
    [Theory]
    [InlineData("<ul><li>item1</li><li>item2</li></ul>", "<ul><li>item1</li><li>item2</li></ul>")]
    [InlineData("<UL><LI>item1</LI><LI>item2</LI></UL>", "<ul><li>item1</li><li>item2</li></ul>")]
    [InlineData("<p>paragraphs are not removed</p>", "<p>paragraphs are not removed</p>")]
    [InlineData("br elements are <br> not removed", "br elements are <br> not removed")]
    [InlineData("Windows style carriage\r\nreturns are changed", "Windows style carriage\nreturns are changed")]
    [InlineData("<div>any other elements are not allowed</div>", "")]
    [InlineData("<ul><li onclick=\"alert('not allowed')\">item1</li><li>item2</li></ul>", "<ul><li>item1</li><li>item2</li></ul>")]
    [InlineData("<ul><li style=\"display: none\">item1</li><li>item2</li></ul>", "<ul><li>item1</li><li>item2</li></ul>")]
    [InlineData("<script>alert('not allowed')</script>", "")]
    [InlineData("<style>body { display:none}</style>", "")]
    public void Sanitize_ShouldSanitizeHtml(string unsanitized, string sanitized)
    {
        var htmlSanitizer = new HtmlSanitizerService(new Mock<ILogger<HtmlSanitizerService>>().Object);

        var actual = htmlSanitizer.Sanitize(unsanitized);

        Assert.Equal(sanitized, actual);
    }

    [Theory]
    [InlineData("<ul><li>item1</li><li>item2</li></ul>", true)]
    [InlineData("<UL><LI>item1</LI><LI>item2</LI></UL>", true)]
    [InlineData("<p>paragraphs are not removed</p>", true)]
    [InlineData("br elements are <br> not removed", true)]
    [InlineData("Windows style carriage\r\nreturns are ok", true)]
    [InlineData("", true)]
    [InlineData(null, true)]
    [InlineData("<div>any other elements are not allowed</div>", false)]
    [InlineData("<ul><li onclick=\"alert('not allowed')\">item1</li><li>item2</li></ul>", false)]
    [InlineData("<ul><li style=\"display: none\">item1</li><li>item2</li></ul>", false)]
    [InlineData("<script>alert('not allowed')</script>", false)]
    [InlineData("<style>body { display:none}</style>", false)]
    [InlineData("<p>\r\n&nbsp;<br>&nbsp;<p><p></p>", false)]
    public void IsValid_ShouldValidateHtml(string value, bool isValid)
    {
        var htmlSanitizer = new HtmlSanitizerService(new Mock<ILogger<HtmlSanitizerService>>().Object);

        var actual = htmlSanitizer.IsValid(value);

        Assert.Equal(isValid, actual);
    }
}