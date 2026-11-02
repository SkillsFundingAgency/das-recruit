using System;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Domain.Entities;

public class ApprenticeshipLevelHelperTests
{
    [Fact]
    public void RemapFromInt_ShouldReturnHigher_WhenConvertingFromFoundation5()
    {
        ApprenticeshipLevel result = ApprenticeshipLevelHelper.RemapFromInt(5);
        result.Should().Be(ApprenticeshipLevel.Higher);
    }

    [Fact]
    public void RemapFromInt_ShouldReturnDegree_WhenConvertingFromMasters7()
    {
        ApprenticeshipLevel result = ApprenticeshipLevelHelper.RemapFromInt(7);
        result.Should().Be(ApprenticeshipLevel.Degree);
    }

    [Fact]
    public void RemapFromInt_ShouldReturnCorrectEnum_WhenPassedAnIntWithCorrespondingValue()
    {
        var enumValues = Enum.GetValues(typeof(ApprenticeshipLevel))
            .OfType<ApprenticeshipLevel>();
        foreach (ApprenticeshipLevel enumValue in enumValues)
        {
            ApprenticeshipLevel result = ApprenticeshipLevelHelper.RemapFromInt((int)enumValue);
            result.Should().Be(enumValue);
        }
    }

    [Fact]
    public void RemapFromInt_ShouldThrowArgumentException_WhenPassedAnIntWithNoCorrespondingValue()
    {
        int intValueToConvert = (int)ApprenticeshipLevel.Degree + 2;
        Assert.Throws<ArgumentException>(() =>
        {
            ApprenticeshipLevelHelper.RemapFromInt(intValueToConvert);
        });
    }

    [Fact]
    public void TryRemapFromInt_ShouldSucceedAndReturnHigher_WhenConvertingFromFoundation5()
    {
        bool success = ApprenticeshipLevelHelper.TryRemapFromInt(5, out ApprenticeshipLevel result);
        success.Should().Be(true);
        result.Should().Be(ApprenticeshipLevel.Higher);
    }

    [Fact]
    public void TryRemapFromInt_ShouldSucceedAndReturnDegree_WhenConvertingFromMasters7()
    {
        bool success = ApprenticeshipLevelHelper.TryRemapFromInt(7, out ApprenticeshipLevel result);
        success.Should().Be(true);
        result.Should().Be(ApprenticeshipLevel.Degree);
    }

    [Fact]
    public void TryRemapFromInt_ShouldSucceedAndReturnEnum_WhenPassedAnIntWithCorrespondingValue()
    {
        var enumValues = Enum.GetValues(typeof(ApprenticeshipLevel))
            .OfType<ApprenticeshipLevel>();
        foreach (ApprenticeshipLevel enumValue in enumValues)
        {
            bool success = ApprenticeshipLevelHelper.TryRemapFromInt((int)enumValue, out ApprenticeshipLevel result);
            success.Should().Be(true);
            result.Should().Be(enumValue);
        }
    }

    [Fact]
    public void TryRemapFromInt_ShouldFail_WhenPassedAnIntWithNoCorrespondingValue()
    {
        int intValueToConvert = (int)ApprenticeshipLevel.Degree + 2;
        bool success = ApprenticeshipLevelHelper.TryRemapFromInt(intValueToConvert, out ApprenticeshipLevel result);
        success.Should().Be(false);
        result.Should().Be(ApprenticeshipLevel.Unknown);
    }

}