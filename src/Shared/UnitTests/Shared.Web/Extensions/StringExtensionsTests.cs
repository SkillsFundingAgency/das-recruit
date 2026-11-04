using NUnit.Framework;
using Esfa.Recruit.Shared.Web.Extensions;
using FluentAssertions;

namespace Esfa.Recruit.Shared.Web.UnitTests.Extensions;

public class StringExtensionsTests
{
    [Test]
    [TestCase("SW1A 2AA", "SW1A 2AA")]
    [TestCase(" sw 1a2aa ", "SW1A 2AA" )]
    [TestCase(null, null)]
    public void AsPostcode_ShouldFormatText(string actualPostcode, string expectedPostcode)
    {
        actualPostcode.AsPostcode().Should().Be(expectedPostcode);
    }
}
