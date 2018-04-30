using System;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Application.Services.MinimumWage;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.Shared.Web.UnitTests.Extensions
{
    public class WageExtensionsTests
    {

        private readonly Func<WageRange> _getNationalMinimumWage = () => new WageRange{MinimumWage = 4.05m, MaximumWage = 7.83m};
        private readonly Func<decimal> _getApprenticeNationalMinimumWage = () => 3.70m;

        [Fact]
        public void ShouldFormatFixedWageCorrectly()
        {
            var wage = new Wage
            {
                WageType = WageType.FixedWage,
                FixedWageYearlyAmount = 12345678.91m
            };

            var actual = wage.ToText(null, null);

            actual.Should().Be("£12,345,678.91");
        }

        [Fact]
        public void ShouldFormatNationalMinimumWageCorrectly()
        {
            var wage = new Wage
            {
                WageType = WageType.NationalMinimumWage,
                WeeklyHours = 37.555m
            };

            var actual = wage.ToText(_getNationalMinimumWage, null);

            actual.Should().Be("£7,909.08 - £15,290.89");
        }

        [Fact]
        public void ShouldFormatNationalMinimumWageForApprenticesCorrectly()
        {
            var wage = new Wage
            {
                WageType = WageType.NationalMinimumWageForApprentices,
                WeeklyHours = 37.555m
            };

            var actual = wage.ToText(null, _getApprenticeNationalMinimumWage);

            actual.Should().Be("£7,225.58");
        }

        [Fact]
        public void ShouldFormatUnspecifiedWageCorrectly()
        {
            var wage = new Wage
            {
                WageType = WageType.Unspecified
            };

            var actual = wage.ToText(null, null);

            actual.Should().Be("Unspecified");
        }
    }
}
