using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.Services
{
    public class HtmlValidationTests
    {
        [Theory]
        [InlineData("<ul><li>item1</li><li>item2</li></ul>", "<ul><li>item1</li><li>item2</li></ul>")]
        [InlineData("<UL><LI>item1</LI><LI>item2</LI></UL>", "<ul><li>item1</li><li>item2</li></ul>")]
        [InlineData("<p>paragraphs are not removed</p>", "<p>paragraphs are not removed</p>")]
        [InlineData("br elements are <br> not removed", "br elements are <br> not removed")]
        [InlineData("<div>any other elements are not allowed</div>", "")]
        [InlineData("<ul><li onclick=\"alert('not allowed')\">item1</li><li>item2</li></ul>", "<ul><li>item1</li><li>item2</li></ul>")]
        [InlineData("<ul><li style=\"display: none\">item1</li><li>item2</li></ul>", "<ul><li>item1</li><li>item2</li></ul>")]
        [InlineData("<script>alert('not allowed')</script>", "")]
        [InlineData("<style>body { display:none}</style>", "")]
        public void DescriptionMustContainValidHtml(string unsanitized, string sanitized)
        {
            var htmlSanitizer = new HtmlSanitizerService(new Mock<ILogger<HtmlSanitizerService>>().Object);

            var actual = htmlSanitizer.Sanitize(unsanitized);

            Assert.Equal(sanitized, actual);
        }
    }
}
