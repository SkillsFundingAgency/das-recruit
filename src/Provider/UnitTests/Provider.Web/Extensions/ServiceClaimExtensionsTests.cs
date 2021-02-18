using Esfa.Recruit.Provider.Web.Extensions;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Extensions
{
    public class ServiceClaimExtensionsTests
    {
        [Theory]
        [InlineData("DAA", true)]
        [InlineData("DAB", true)]
        [InlineData("DAC", true)]
        [InlineData("DAV", true)]
        [InlineData("DA", false)]
        [InlineData("FAKE", false)]
        public void ValidateServiceClaimsTest(string claim, bool expected)
        {
            bool result = claim.IsServiceClaim();
            result.Should().Be(expected);
        }
    }
}