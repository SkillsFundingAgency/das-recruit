using Esfa.Recruit.Vacancies.Client.Application.Services;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Esfa.Recruit.UnitTests.Vacancies.Client.Application.Services;

public class HtmlSanitizerServiceTests
{
    [Test]
    [TestCase("<ul><li>item1</li><li>item2</li></ul>", "<ul><li>item1</li><li>item2</li></ul>")]
    [TestCase("<UL><LI>item1</LI><LI>item2</LI></UL>", "<ul><li>item1</li><li>item2</li></ul>")]
    [TestCase("<p>paragraphs are not removed</p>", "<p>paragraphs are not removed</p>")]
    [TestCase("br elements are <br> not removed", "br elements are <br> not removed")]
    [TestCase("Windows style carriage\r\nreturns are changed", "Windows style carriage\nreturns are changed")]
    [TestCase("<div>any other elements are not allowed</div>", "")]
    [TestCase("<ul><li onclick=\"alert('not allowed')\">item1</li><li>item2</li></ul>", "<ul><li>item1</li><li>item2</li></ul>")]
    [TestCase("<ul><li style=\"display: none\">item1</li><li>item2</li></ul>", "<ul><li>item1</li><li>item2</li></ul>")]
    [TestCase("<script>alert('not allowed')</script>", "")]
    [TestCase("<style>body { display:none}</style>", "")]
    public void Sanitize_ShouldSanitizeHtml(string unsanitized, string sanitized)
    {
        var htmlSanitizer = new HtmlSanitizerService(new Mock<ILogger<HtmlSanitizerService>>().Object);

        var actual = htmlSanitizer.Sanitize(unsanitized);

        sanitized.Should().Be(actual);
    }

    [Test]
    [TestCase("<ul><li>item1</li><li>item2</li></ul>", true)]
    [TestCase("<UL><LI>item1</LI><LI>item2</LI></UL>", true)]
    [TestCase("<p>paragraphs are not removed</p>", true)]
    [TestCase("br elements are <br> not removed", true)]
    [TestCase("Windows style carriage\r\nreturns are ok", true)]
    [TestCase("", true)]
    [TestCase(null, true)]
    [TestCase("<div>any other elements are not allowed</div>", false)]
    [TestCase("<ul><li onclick=\"alert('not allowed')\">item1</li><li>item2</li></ul>", false)]
    [TestCase("<ul><li style=\"display: none\">item1</li><li>item2</li></ul>", false)]
    [TestCase("<script>alert('not allowed')</script>", false)]
    [TestCase("<style>body { display:none}</style>", false)]
    [TestCase("<p>\r\n&nbsp;<br>&nbsp;<p><p></p>", false)]
    public void IsValid_ShouldValidateHtml(string value, bool isValid)
    {
        var htmlSanitizer = new HtmlSanitizerService(new Mock<ILogger<HtmlSanitizerService>>().Object);

        var actual = htmlSanitizer.IsValid(value);

        isValid.Should().Be(actual);
    }
}