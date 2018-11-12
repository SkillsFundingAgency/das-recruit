using System;
using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.Wages;
using Esfa.Recruit.Vacancies.Client.UnitTests.TestHelpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;
using Moq;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.Services
{
    public class NationalMinimumWageProviderTests
    {
        private readonly NationalMinimumWageProvider _provider;
        private readonly Mock<ILogger<NationalMinimumWageProvider>> _mockLogger;
        
        public NationalMinimumWageProviderTests()
        {
            _mockLogger = new Mock<ILogger<NationalMinimumWageProvider>>();
            var mockReferenceDataReader = new Mock<IReferenceDataReader>();
            mockReferenceDataReader.Setup(x => x.GetReferenceData<MinimumWages>()).ReturnsAsync(GetTestData());

            _provider = new NationalMinimumWageProvider(mockReferenceDataReader.Object, _mockLogger.Object, new TestCache());
        }

        [Fact]
        public void ShouldPickCorrectWageRangeBasedOnDate()
        {
            var testDate = new DateTime(2017, 5, 22);
            var wagePeriod = _provider.GetWagePeriod(testDate);

            wagePeriod.ApprenticeshipMinimumWage.Should().Be(1.65m);
            wagePeriod.NationalMinimumWageLowerBound.Should().Be(1.9m);
            wagePeriod.NationalMinimumWageUpperBound.Should().Be(6.9m);
        }
        
        [Fact]
        public void StartDateShouldBeInclusive()
        {
            var testDate = new DateTime(2018, 4, 1);
            var wagePeriod = _provider.GetWagePeriod(testDate);

            wagePeriod.ApprenticeshipMinimumWage.Should().Be(2.5m);
            wagePeriod.NationalMinimumWageLowerBound.Should().Be(2.9m);
            wagePeriod.NationalMinimumWageUpperBound.Should().Be(7.4m);
        }
        
        [Fact]
        public void EndDateShouldBeInclusive()
        {
            var testDate = new DateTime(2018, 3, 31);
            var wagePeriod = _provider.GetWagePeriod(testDate);

            wagePeriod.ApprenticeshipMinimumWage.Should().Be(1.65m);
            wagePeriod.NationalMinimumWageLowerBound.Should().Be(1.9m);
            wagePeriod.NationalMinimumWageUpperBound.Should().Be(6.9m);
        }
        
        [Fact]
        public void ShouldIgnoreTimePartOfDateTime()
        {
            var testDate = new DateTime(2019, 3, 31, 13, 22, 11);
            var wagePeriod = _provider.GetWagePeriod(testDate);

            wagePeriod.ApprenticeshipMinimumWage.Should().Be(2.5m);
            wagePeriod.NationalMinimumWageLowerBound.Should().Be(2.9m);
            wagePeriod.NationalMinimumWageUpperBound.Should().Be(7.4m);
        }

        [Fact]
        public void IfMultipleMatchesThrowInvalidOperationException()
        {
            var testDate = new DateTime(2022, 6, 24);
            _provider.Invoking(x => x.GetWagePeriod(testDate)).Should().Throw<InvalidOperationException>();

            _mockLogger.Verify(
                m => m.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<FormattedLogValues>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<object, Exception, string>>()
                )
            );
        }

        [Fact]
        public void IfNoMatchesThrowInvalidOperationException()
        {
            var testDate = new DateTime(2015, 1, 1);
            _provider.Invoking(x => x.GetWagePeriod(testDate)).Should().Throw<InvalidOperationException>();
            
            _mockLogger.Verify(
                m => m.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<FormattedLogValues>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<object, Exception, string>>()
                )
            );
        }

        private static MinimumWages GetTestData()
        {
            var minimumWages = new MinimumWages 
            {
                Ranges = new List<MinimumWage>
                {
                    new MinimumWage
                    {
                        ValidFrom = new DateTime(2016, 4, 1),
                        ApprenticeshipMinimumWage = 1.34m,
                        NationalMinimumWageLowerBound = 1.7m,
                        NationalMinimumWageUpperBound = 6.4m
                    },
                    new MinimumWage
                    {
                        ValidFrom = new DateTime(2017, 4, 1),
                        ApprenticeshipMinimumWage = 1.65m,
                        NationalMinimumWageLowerBound = 1.9m,
                        NationalMinimumWageUpperBound = 6.9m
                    },
                    new MinimumWage
                    {
                        ValidFrom = new DateTime(2018, 4, 1),
                        ApprenticeshipMinimumWage = 2.5m,
                        NationalMinimumWageLowerBound = 2.9m,
                        NationalMinimumWageUpperBound = 7.4m
                    },
                    new MinimumWage
                    {
                        ValidFrom = new DateTime(2019, 4, 1),
                        ApprenticeshipMinimumWage = 3.90m,
                        NationalMinimumWageLowerBound = 4.35m,
                        NationalMinimumWageUpperBound = 8.21m
                    },
                    new MinimumWage // Duplicate entry
                    {
                        ValidFrom = new DateTime(2019, 4, 1),
                        ApprenticeshipMinimumWage = 3.90m,
                        NationalMinimumWageLowerBound = 4.35m,
                        NationalMinimumWageUpperBound = 8.21m
                    }
                }
            };

            return minimumWages;
        }
    }
}