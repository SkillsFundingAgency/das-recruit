using System;
using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.Wages;
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

            _provider = new NationalMinimumWageProvider(mockReferenceDataReader.Object, _mockLogger.Object);
        }

        [Fact]
        public void ShouldPickCorrectWageRangeBasedOnDate()
        {
            var testDate = new DateTime(2017, 5, 22);
            var apprenticeshipMinimumWage = _provider.GetApprenticeNationalMinimumWage(testDate);
            var nationMinimumWage = _provider.GetNationalMinimumWageRange(testDate);

            apprenticeshipMinimumWage.Should().Be(1.65m);
            nationMinimumWage.MinimumWage.Should().Be(1.9m);            
            nationMinimumWage.MaximumWage.Should().Be(6.9m);
        }
        
        [Fact]
        public void StartDateShouldBeInclusive()
        {
            var testDate = new DateTime(2018, 4, 1);
            var apprenticeshipMinimumWage = _provider.GetApprenticeNationalMinimumWage(testDate);
            var nationMinimumWage = _provider.GetNationalMinimumWageRange(testDate);

            apprenticeshipMinimumWage.Should().Be(2.5m);
            nationMinimumWage.MinimumWage.Should().Be(2.9m);            
            nationMinimumWage.MaximumWage.Should().Be(7.4m);
        }
        
        [Fact]
        public void EndDateShouldBeInclusive()
        {
            var testDate = new DateTime(2018, 3, 31);
            var apprenticeshipMinimumWage = _provider.GetApprenticeNationalMinimumWage(testDate);
            var nationMinimumWage = _provider.GetNationalMinimumWageRange(testDate);

            apprenticeshipMinimumWage.Should().Be(1.65m);
            nationMinimumWage.MinimumWage.Should().Be(1.9m);            
            nationMinimumWage.MaximumWage.Should().Be(6.9m);
        }
        
        [Fact]
        public void ShouldIgnoreTimePartOfDateTime()
        {
            var testDate = new DateTime(2019, 3, 31, 13, 22, 11);
            var apprenticeshipMinimumWage = _provider.GetApprenticeNationalMinimumWage(testDate);
            var nationMinimumWage = _provider.GetNationalMinimumWageRange(testDate);

            apprenticeshipMinimumWage.Should().Be(2.5m);
            nationMinimumWage.MinimumWage.Should().Be(2.9m);            
            nationMinimumWage.MaximumWage.Should().Be(7.4m);
        }
        
        [Fact]
        public void IfMultipelMatchesThrowInvalidOperationException()
        {
            var testDate = new DateTime(2022, 6, 24);
            _provider.Invoking(x => x.GetNationalMinimumWageRange(testDate)).Should().Throw<InvalidOperationException>();
            
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
            var testDate = new DateTime(2022, 6, 24);
            _provider.Invoking(x => x.GetNationalMinimumWageRange(testDate)).Should().Throw<InvalidOperationException>();
            
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
                        ValidTo = new DateTime(2017, 3, 31),
                        ApprenticeshipMinimumWage = 1.34m,
                        NationalMinimumWageLowerBound = 1.7m,
                        NationalMinimumWageUpperBound = 6.4m
                    },
                    new MinimumWage
                    {
                        ValidFrom = new DateTime(2017, 4, 1),
                        ValidTo = new DateTime(2018, 3, 31),
                        ApprenticeshipMinimumWage = 1.65m,
                        NationalMinimumWageLowerBound = 1.9m,
                        NationalMinimumWageUpperBound = 6.9m
                    },
                    new MinimumWage
                    {
                        ValidFrom = new DateTime(2018, 4, 1),
                        ValidTo = new DateTime(2019, 3, 31),
                        ApprenticeshipMinimumWage = 2.5m,
                        NationalMinimumWageLowerBound = 2.9m,
                        NationalMinimumWageUpperBound = 7.4m
                    },
                    new MinimumWage
                    {
                        ValidFrom = new DateTime(2019, 4, 1),
                        ValidTo = new DateTime(2020, 3, 31),
                        ApprenticeshipMinimumWage = 3.34m,
                        NationalMinimumWageLowerBound = 3.7m,
                        NationalMinimumWageUpperBound = 8.2m
                    },
                    new MinimumWage // Duplicate entry
                    {
                        ValidFrom = new DateTime(2019, 4, 1),
                        ValidTo = new DateTime(2020, 3, 31),
                        ApprenticeshipMinimumWage = 3.34m,
                        NationalMinimumWageLowerBound = 3.7m,
                        NationalMinimumWageUpperBound = 8.2m
                    }
                }
            };

            return minimumWages;
        }
    }
}