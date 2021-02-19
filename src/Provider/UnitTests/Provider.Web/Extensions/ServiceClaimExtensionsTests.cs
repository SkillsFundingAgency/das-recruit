using System.Collections.Generic;
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
        [InlineData(ServiceClaim.DAA, true)]
        [InlineData(ServiceClaim.DAB, true)]
        [InlineData(ServiceClaim.DAC, true)]
        [InlineData(ServiceClaim.DAV, true)]
        public void ShouldReturnTheHighestClaim_DAA(ServiceClaim minimumServiceClaim, bool expected)
        {
            var claims = new List<Claim>
            {
                new Claim(ProviderRecruitClaims.IdamsUserServiceTypeClaimTypeIdentifier, "DAV"),
                new Claim(ProviderRecruitClaims.IdamsUserServiceTypeClaimTypeIdentifier, "DAA"),
                new Claim(ProviderRecruitClaims.IdamsUserServiceTypeClaimTypeIdentifier, "DAB"),
                new Claim(ProviderRecruitClaims.IdamsUserServiceTypeClaimTypeIdentifier, "DAC")
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            var result = claimsPrincipal.HasPermission(minimumServiceClaim);
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData(ServiceClaim.DAA, true)]
        [InlineData(ServiceClaim.DAB, true)]
        [InlineData(ServiceClaim.DAC, true)]
        [InlineData(ServiceClaim.DAV, true)]
        public void ShouldReturnHasPermission_DAA(ServiceClaim minimumServiceClaim, bool expected)
        {
            var claims = new List<Claim>
            {
                new Claim(ProviderRecruitClaims.IdamsUserServiceTypeClaimTypeIdentifier, "DAV"),
                new Claim(ProviderRecruitClaims.IdamsUserServiceTypeClaimTypeIdentifier, "DAA"),
                new Claim(ProviderRecruitClaims.IdamsUserServiceTypeClaimTypeIdentifier, "DAB"),
                new Claim(ProviderRecruitClaims.IdamsUserServiceTypeClaimTypeIdentifier, "DAC")
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            var result = claimsPrincipal.HasPermission(minimumServiceClaim);
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData(ServiceClaim.DAA, false)]
        [InlineData(ServiceClaim.DAB, true)]
        [InlineData(ServiceClaim.DAC, true)]
        [InlineData(ServiceClaim.DAV, true)]
        public void ShouldReturnHasPermission_DAB(ServiceClaim minimumServiceClaim, bool expected)
        {
            var claims = new List<Claim>
            {
                new Claim(ProviderRecruitClaims.IdamsUserServiceTypeClaimTypeIdentifier, "DAV"),
                new Claim(ProviderRecruitClaims.IdamsUserServiceTypeClaimTypeIdentifier, "DAB"),
                new Claim(ProviderRecruitClaims.IdamsUserServiceTypeClaimTypeIdentifier, "DAC")
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            var result = claimsPrincipal.HasPermission(minimumServiceClaim);
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData(ServiceClaim.DAA, false)]
        [InlineData(ServiceClaim.DAB, false)]
        [InlineData(ServiceClaim.DAC, true)]
        [InlineData(ServiceClaim.DAV, true)]
        public void ShouldReturnHasPermission_DAC(ServiceClaim minimumServiceClaim, bool expected)
        {
            var claims = new List<Claim>
            {
                new Claim(ProviderRecruitClaims.IdamsUserServiceTypeClaimTypeIdentifier, "DAV"),
                new Claim(ProviderRecruitClaims.IdamsUserServiceTypeClaimTypeIdentifier, "DAC")
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            var result = claimsPrincipal.HasPermission(minimumServiceClaim);
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData(ServiceClaim.DAA, false)]
        [InlineData(ServiceClaim.DAB, false)]
        [InlineData(ServiceClaim.DAC, false)]
        [InlineData(ServiceClaim.DAV, true)]
        public void ShouldReturnHasPermission_DAV(ServiceClaim minimumServiceClaim, bool expected)
        {
            var claims = new List<Claim>
            {
                new Claim(ProviderRecruitClaims.IdamsUserServiceTypeClaimTypeIdentifier, "DAV")
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            var result = claimsPrincipal.HasPermission(minimumServiceClaim);
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData(ServiceClaim.DAA, false)]
        [InlineData(ServiceClaim.DAB, false)]
        [InlineData(ServiceClaim.DAC, false)]
        [InlineData(ServiceClaim.DAV, false)]
        public void ShouldReturnHasPermission_NULL(ServiceClaim minimumServiceClaim, bool expected)
        {
            var claims = new List<Claim>();

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            var result = claimsPrincipal.HasPermission(minimumServiceClaim);
            result.Should().Be(expected);
        }
    }
}