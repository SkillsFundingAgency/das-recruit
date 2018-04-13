using System;
using System.Collections.Generic;
using System.Text;
using Esfa.Recruit.Employer.Web.Extensions;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Extensions
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
