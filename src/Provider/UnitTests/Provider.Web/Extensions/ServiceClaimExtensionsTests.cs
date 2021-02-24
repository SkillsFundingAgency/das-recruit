using System.Linq;
using System.Security.Claims;
using Esfa.Recruit.Provider.Web.Configuration;
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

        [Theory]
        [InlineData(ServiceClaim.DAA, new ServiceClaim[] {ServiceClaim.DAA, ServiceClaim.DAB, ServiceClaim.DAC, ServiceClaim.DAV}, true)]
        [InlineData(ServiceClaim.DAB, new ServiceClaim[] {ServiceClaim.DAA, ServiceClaim.DAB, ServiceClaim.DAC, ServiceClaim.DAV}, true)]
        [InlineData(ServiceClaim.DAC, new ServiceClaim[] {ServiceClaim.DAA, ServiceClaim.DAB, ServiceClaim.DAC, ServiceClaim.DAV}, true)]
        [InlineData(ServiceClaim.DAV, new ServiceClaim[] {ServiceClaim.DAA, ServiceClaim.DAB, ServiceClaim.DAC, ServiceClaim.DAV}, true)]
        [InlineData(ServiceClaim.DAA, new ServiceClaim[] {ServiceClaim.DAB, ServiceClaim.DAC, ServiceClaim.DAV}, false)]
        [InlineData(ServiceClaim.DAB, new ServiceClaim[] {ServiceClaim.DAB, ServiceClaim.DAC, ServiceClaim.DAV}, true)]
        [InlineData(ServiceClaim.DAC, new ServiceClaim[] {ServiceClaim.DAB, ServiceClaim.DAC, ServiceClaim.DAV}, true)]
        [InlineData(ServiceClaim.DAV, new ServiceClaim[] {ServiceClaim.DAB, ServiceClaim.DAC, ServiceClaim.DAV}, true)]
        [InlineData(ServiceClaim.DAA, new ServiceClaim[] {ServiceClaim.DAC, ServiceClaim.DAV}, false)]
        [InlineData(ServiceClaim.DAB, new ServiceClaim[] {ServiceClaim.DAC, ServiceClaim.DAV}, false)]
        [InlineData(ServiceClaim.DAC, new ServiceClaim[] {ServiceClaim.DAC, ServiceClaim.DAV}, true)]
        [InlineData(ServiceClaim.DAV, new ServiceClaim[] {ServiceClaim.DAC, ServiceClaim.DAV}, true)]
        [InlineData(ServiceClaim.DAA, new ServiceClaim[] {ServiceClaim.DAA}, true)]
        [InlineData(ServiceClaim.DAB, new ServiceClaim[] {ServiceClaim.DAA}, true)]
        [InlineData(ServiceClaim.DAC, new ServiceClaim[] {ServiceClaim.DAA}, true)]
        [InlineData(ServiceClaim.DAV, new ServiceClaim[] {ServiceClaim.DAA}, true)]
        [InlineData(ServiceClaim.DAA, new ServiceClaim[] {ServiceClaim.DAB}, false)]
        [InlineData(ServiceClaim.DAB, new ServiceClaim[] {ServiceClaim.DAB}, true)]
        [InlineData(ServiceClaim.DAC, new ServiceClaim[] {ServiceClaim.DAB}, true)]
        [InlineData(ServiceClaim.DAV, new ServiceClaim[] {ServiceClaim.DAB}, true)]
        [InlineData(ServiceClaim.DAA, new ServiceClaim[] {ServiceClaim.DAC}, false)]
        [InlineData(ServiceClaim.DAB, new ServiceClaim[] {ServiceClaim.DAC}, false)]
        [InlineData(ServiceClaim.DAC, new ServiceClaim[] {ServiceClaim.DAC}, true)]
        [InlineData(ServiceClaim.DAV, new ServiceClaim[] {ServiceClaim.DAC}, true)]
        [InlineData(ServiceClaim.DAA, new ServiceClaim[] {ServiceClaim.DAV}, false)]
        [InlineData(ServiceClaim.DAB, new ServiceClaim[] {ServiceClaim.DAV}, false)]
        [InlineData(ServiceClaim.DAC, new ServiceClaim[] {ServiceClaim.DAV}, false)]
        [InlineData(ServiceClaim.DAV, new ServiceClaim[] {ServiceClaim.DAV}, true)]
        [InlineData(ServiceClaim.DAA, new ServiceClaim[] { }, false)]
        [InlineData(ServiceClaim.DAB, new ServiceClaim[] { }, false)]
        [InlineData(ServiceClaim.DAC, new ServiceClaim[] { }, false)]
        [InlineData(ServiceClaim.DAV, new ServiceClaim[] { }, false)]
        public void ShouldReturnHasPermission(ServiceClaim minimumServiceClaim, ServiceClaim[] actualServiceClaims, bool expected)
        {
            var claims = actualServiceClaims.Select(a => new Claim(ProviderRecruitClaims.IdamsUserServiceTypeClaimTypeIdentifier, a.ToString()));

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            bool result = claimsPrincipal.HasPermission(minimumServiceClaim);
            result.Should().Be(expected);
        }
    }
}