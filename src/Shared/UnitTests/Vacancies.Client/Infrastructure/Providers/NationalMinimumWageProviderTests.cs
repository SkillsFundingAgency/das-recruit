using System;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Esfa.Recruit.UnitTests.Vacancies.Client.Infrastructure.Services;

public class NationalMinimumWageProviderTests
{
    private readonly NationalMinimumWageProvider _provider;
    private readonly Mock<ILogger<NationalMinimumWageProvider>> _mockLogger;
        
    public NationalMinimumWageProviderTests()
    {
        _mockLogger = new Mock<ILogger<NationalMinimumWageProvider>>();
        _provider = new NationalMinimumWageProvider(_mockLogger.Object);
    }

    [Test]
    public void ShouldPickCorrectWageRangeBasedOnDate()
    {
        var testDate = new DateTime(2017, 5, 22);
        var wagePeriod = _provider.GetWagePeriod(testDate);

        wagePeriod.ApprenticeshipMinimumWage.Should().Be(3.50m);
        wagePeriod.NationalMinimumWageLowerBound.Should().Be(4.05m);
        wagePeriod.NationalMinimumWageUpperBound.Should().Be(7.50m);
    }
        
    [Test]
    public void StartDateShouldBeInclusive()
    {
        var testDate = new DateTime(2018, 4, 1);
        var wagePeriod = _provider.GetWagePeriod(testDate);

        wagePeriod.ApprenticeshipMinimumWage.Should().Be(3.70m);
        wagePeriod.NationalMinimumWageLowerBound.Should().Be(4.20m);
        wagePeriod.NationalMinimumWageUpperBound.Should().Be(7.83m);
    }
        
    [Test]
    public void EndDateShouldBeInclusive()
    {
        var testDate = new DateTime(2018, 3, 31);
        var wagePeriod = _provider.GetWagePeriod(testDate);

        wagePeriod.ApprenticeshipMinimumWage.Should().Be(3.50m);
        wagePeriod.NationalMinimumWageLowerBound.Should().Be(4.05m);
        wagePeriod.NationalMinimumWageUpperBound.Should().Be(7.50m);
    }
        
    [Test]
    public void ShouldIgnoreTimePartOfDateTime()
    {
        var testDate = new DateTime(2018, 3, 31, 13, 22, 11);
        var wagePeriod = _provider.GetWagePeriod(testDate);

        wagePeriod.ApprenticeshipMinimumWage.Should().Be(3.5m);
        wagePeriod.NationalMinimumWageLowerBound.Should().Be(4.05m);
        wagePeriod.NationalMinimumWageUpperBound.Should().Be(7.5m);
    }
}
