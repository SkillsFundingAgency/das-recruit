using Xunit;
using Esfa.Recruit.Shared.Web.Extensions;
using FluentAssertions;

namespace Esfa.Recruit.Shared.Web.UnitTests.Extensions
{
    public class StringExtensionsTests
    {
        [Theory]
        [InlineData("SW1A 2AA", "SW1A 2AA")]
        [InlineData(" sw 1a2aa ", "SW1A 2AA" )]
        [InlineData(null, null)]
        public void AsPostcode_ShouldFormatText(string actualPostcode, string expectedPostcode)
        {
            actualPostcode.AsPostcode().Should().Be(expectedPostcode);
        }
    }
}
