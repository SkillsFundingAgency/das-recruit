using System;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.Wages;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.Shared.Web.UnitTests.Extensions
{
    public class WageExtensionsTests
    {
        private readonly IMinimumWage _wagePeriod = new MinimumWage
        {
            ApprenticeshipMinimumWage = 3.70m,
            NationalMinimumWageLowerBound = 4.05m,
            NationalMinimumWageUpperBound = 7.83m
        };
        
        [Fact]
        public void ShouldFormatFixedWageCorrectly()
        {
            var wage = new Wage
            {
                WageType = WageType.FixedWage,
                FixedWageYearlyAmount = 12345678.91m
            };

            var actual = wage.ToText(_wagePeriod);

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

            var actual = wage.ToText(_wagePeriod);

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

            var actual = wage.ToText(_wagePeriod);

            actual.Should().Be("£7,225.58");
        }

        [Fact]
        public void ShouldFormatUnspecifiedWageCorrectly()
        {
            var wage = new Wage
            {
                WageType = WageType.Unspecified
            };

            var actual = wage.ToText(_wagePeriod);

            actual.Should().Be("Unspecified");
        }
    }
}
