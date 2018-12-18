﻿using System;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.Shared.Web.UnitTests.Extensions
{
    public class WageExtensionsTests
    {
        [Fact]
        public void ShouldFormatFixedWageCorrectly()
        {
            var wage = new Wage
            {
                WageType = WageType.FixedWage,
                FixedWageYearlyAmount = 12345678.91m
            };

            var actual = wage.ToText(null);

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

            var actual = wage.ToText(new DateTime(2018, 5, 1));

            actual.Should().Be("£8,202.01 - £15,290.89");
        }

        [Fact]
        public void ShouldFormatNationalMinimumWageForApprenticesCorrectly()
        {
            var wage = new Wage
            {
                WageType = WageType.NationalMinimumWageForApprentices,
                WeeklyHours = 37.555m
            };

            var actual = wage.ToText(new DateTime(2018, 5, 1));

            actual.Should().Be("£7,225.58");
        }

        [Fact]
        public void ShouldFormatUnspecifiedWageCorrectly()
        {
            var wage = new Wage
            {
                WageType = WageType.Unspecified
            };

            var actual = wage.ToText(null);

            actual.Should().Be("Unspecified");
        }
    }
}
