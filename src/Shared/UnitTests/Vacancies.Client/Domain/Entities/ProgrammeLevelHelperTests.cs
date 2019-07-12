using System;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Domain.Entities
{
    public class ProgrammeLevelHelperTests
    {
        [Fact]
        public void RemapFromInt_ShouldReturnHigher_WhenConvertingFromFoundation5()
        {
            ProgrammeLevel result = ProgrammeLevelHelper.RemapFromInt(5);
            result.Should().Be(ProgrammeLevel.Higher);
        }

        [Fact]
        public void RemapFromInt_ShouldReturnDegree_WhenConvertingFromMasters7()
        {
            ProgrammeLevel result = ProgrammeLevelHelper.RemapFromInt(7);
            result.Should().Be(ProgrammeLevel.Degree);
        }

        [Fact]
        public void RemapFromInt_ShouldReturnCorrectEnum_WhenPassedAnIntWithCorrespondingValue()
        {
            var enumValues = Enum.GetValues(typeof(ProgrammeLevel))
                .OfType<ProgrammeLevel>();
            foreach (ProgrammeLevel enumValue in enumValues)
            {
                ProgrammeLevel result = ProgrammeLevelHelper.RemapFromInt((int)enumValue);
                result.Should().Be(enumValue);
            }
        }

        [Fact]
        public void RemapFromInt_ShouldThrowArgumentException_WhenPassedAnIntWithNoCorrespondingValue()
        {
            int intValueToConvert = (int)ProgrammeLevel.Degree + 2;
            Assert.Throws<ArgumentException>(() =>
            {
                ProgrammeLevelHelper.RemapFromInt(intValueToConvert);
            });
        }

        [Fact]
        public void TryRemapFromInt_ShouldSucceedAndReturnHigher_WhenConvertingFromFoundation5()
        {
            bool success = ProgrammeLevelHelper.TryRemapFromInt(5, out ProgrammeLevel result);
            success.Should().Be(true);
            result.Should().Be(ProgrammeLevel.Higher);
        }

        [Fact]
        public void TryRemapFromInt_ShouldSucceedAndReturnDegree_WhenConvertingFromMasters7()
        {
            bool success = ProgrammeLevelHelper.TryRemapFromInt(7, out ProgrammeLevel result);
            success.Should().Be(true);
            result.Should().Be(ProgrammeLevel.Degree);
        }

        [Fact]
        public void TryRemapFromInt_ShouldSucceedAndReturnEnum_WhenPassedAnIntWithCorrespondingValue()
        {
            var enumValues = Enum.GetValues(typeof(ProgrammeLevel))
                .OfType<ProgrammeLevel>();
            foreach (ProgrammeLevel enumValue in enumValues)
            {
                bool success = ProgrammeLevelHelper.TryRemapFromInt((int)enumValue, out ProgrammeLevel result);
                success.Should().Be(true);
                result.Should().Be(enumValue);
            }
        }

        [Fact]
        public void TryRemapFromInt_ShouldFail_WhenPassedAnIntWithNoCorrespondingValue()
        {
            int intValueToConvert = (int)ProgrammeLevel.Degree + 2;
            bool success = ProgrammeLevelHelper.TryRemapFromInt(intValueToConvert, out ProgrammeLevel result);
            success.Should().Be(false);
            result.Should().Be(ProgrammeLevel.Unknown);
        }

    }
}
