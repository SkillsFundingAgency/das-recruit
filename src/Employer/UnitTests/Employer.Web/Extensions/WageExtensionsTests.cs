using System;
using System.Collections.Generic;
using System.Text;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Extensions
{
    public class WageExtensionsTests
    {

        private Func<Tuple<decimal, decimal>> _getNationalMinimumWage = () => new Tuple<decimal, decimal>(4.05m, 7.83m);
        private Func<decimal> _getApprenticeNationalMinimumWage = () => 3.70m;

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
                WeeklyHours = 37.5m
            };

            var actual = wage.ToText(_getNationalMinimumWage, null);

            actual.Should().Be("£7,897.50 - £15,268.50");
        }

        [Fact]
        public void ShouldFormatNationalMinimumWageForApprenticesCorrectly()
        {
            var wage = new Wage
            {
                WageType = WageType.NationalMinimumWageForApprentices,
                WeeklyHours = 37.5m
            };

            var actual = wage.ToText(null, _getApprenticeNationalMinimumWage);

            actual.Should().Be("£7,215");
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
