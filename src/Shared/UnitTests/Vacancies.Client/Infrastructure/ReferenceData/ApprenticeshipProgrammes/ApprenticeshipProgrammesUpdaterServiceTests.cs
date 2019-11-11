using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Services.ReferenceData;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.ApprenticeshipProgrammes;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Apprenticeships.Api.Client;
using SFA.DAS.Apprenticeships.Api.Types;
using Xunit;
using ApprenticeshipProgrammesReferenceData = Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.ApprenticeshipProgrammes.ApprenticeshipProgrammes;

namespace Esfa.Recruit.UnitTests.Vacancies.Client.Infrastructure.ReferenceData.ApprenticeshipProgrammes
{
    [Trait("Category", "Unit")]
    public class ApprenticeshipProgrammesUpdaterServiceTests
    {
        private readonly IApprenticeshipProgrammesUpdateService _sut;
        private readonly Mock<IStandardApiClient> _mockStandardsClient;
        private readonly Mock<IFrameworkApiClient> _mockFrameworksClient;
        private readonly Mock<IReferenceDataWriter> _mockReferenceDataWriter;

        private readonly StandardSummary _standardOne = new StandardSummary
        {
            Id = "1",
            Title = "Network engineer",
            Uri = "https =//findapprenticeshiptraining-api.sfa.bis.gov.uk/standards/1",
            Duration = 24,
            Level = 4,
            IsPublished = true,
            Ssa1 = 6,
            Ssa2 = 6.1,
            EffectiveFrom = new DateTime(2014, 8, 1),
            IsActiveStandard = true,
            StandardSectorCode = 7
        };
        private readonly StandardSummary _standardTwo = new StandardSummary
        {
            Id = "10",
            Title = "Electrical / electronic technical support engineer",
            Uri = "https =//findapprenticeshiptraining-api.sfa.bis.gov.uk/standards/10",
            Duration = 60,
            Level = 6,
            IsPublished = true,
            Ssa1 = 4,
            Ssa2 = 4.2,
            EffectiveFrom = new DateTime(2014, 8, 1),
            IsActiveStandard = true,
            StandardSectorCode = 7
        };       
        private readonly FrameworkSummary _frameworkOne = new FrameworkSummary
        {
            Id = "401-3-1",
            Uri = "https =//findapprenticeshiptraining-api.sfa.bis.gov.uk/frameworks/401-3-1",
            Title = "Drinks Dispense Systems = Drinks Dispense Systems",
            FrameworkName = "Drinks Dispense Systems",
            PathwayName = "Drinks Dispense Systems",
            ProgType = 3,
            FrameworkCode = 401,
            PathwayCode = 1,
            Level = 2,
            Duration = 0,
            CurrentFundingCap = 0,
            Ssa1 = 7,
            Ssa2 = 7.4,
            EffectiveFrom = new DateTime(2013, 2, 1),
            EffectiveTo = new DateTime(2013, 7, 31),
            IsActiveFramework = false
        };
        private readonly FrameworkSummary _frameworkTwo = new FrameworkSummary
        {
            Id = "403-2-1",
            Uri = "https =//findapprenticeshiptraining-api.sfa.bis.gov.uk/frameworks/403-2-1",
            Title = "Food and Drink = Meat and Poultry Industry Skills",
            FrameworkName = "Food and Drink",
            PathwayName = "Meat and Poultry Industry Skills",
            ProgType = 2,
            FrameworkCode = 403,
            PathwayCode = 1,
            Level = 3,
            Duration = 12,
            CurrentFundingCap = 0,
            Ssa1 = 4,
            Ssa2 = 4.2,
            EffectiveFrom = new DateTime(2013, 2, 1),
            EffectiveTo = new DateTime(2018, 4, 30),
            IsActiveFramework = false,
        };            

        public ApprenticeshipProgrammesUpdaterServiceTests()
        {
            var mockReferenceDataReader = new Mock<IReferenceDataReader>();
            _mockStandardsClient = new Mock<IStandardApiClient>();
            _mockFrameworksClient = new Mock<IFrameworkApiClient>();
            _mockReferenceDataWriter = new Mock<IReferenceDataWriter>();

            _sut = new ApprenticeshipProgrammesUpdateService(Mock.Of<ILogger<ApprenticeshipProgrammesUpdateService>>(),
                                                            _mockStandardsClient.Object,
                                                            _mockFrameworksClient.Object,
                                                            _mockReferenceDataWriter.Object,
                                                            mockReferenceDataReader.Object);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        [InlineData(7)]
        public async Task WhenRemappingFromStandards_ShouldSetEducationLevelNumber(int level)
        {
            _standardOne.Level = level;
            _frameworkOne.Level = level;

            _mockStandardsClient
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(new[] { _standardOne });

            _mockFrameworksClient
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(new[] { _frameworkOne });

            ApprenticeshipProgrammesReferenceData updatedData = null;
            _mockReferenceDataWriter
                .Setup(x => x.UpsertReferenceData(It.IsAny<ApprenticeshipProgrammesReferenceData>()))
                .Callback<ApprenticeshipProgrammesReferenceData>(x => updatedData = x)
                .Returns(Task.CompletedTask);

            await _sut.UpdateApprenticeshipProgrammesAsync();

            updatedData.Data.All(x => x.EducationLevelNumber == level).Should().BeTrue();
        }

        [Fact]
        public async Task WhenZeroStandardsAreRetrievedFromApprenticeshipsApi_UpsertApprenticeshipProgrammesReferencedataThrowInfrastuctureException()
        {
            _mockStandardsClient
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(Enumerable.Empty<StandardSummary>());

            _mockFrameworksClient
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new [] { _frameworkOne });

            Func<Task> asyncFunction = async () => { await _sut.UpdateApprenticeshipProgrammesAsync(); };
            (await asyncFunction.Should().ThrowAsync<InfrastructureException>()).WithMessage("Retrieved 0 standards from the apprenticeships api.");
        }

        [Fact]
        public async Task WhenZeroFrameworksAreRetrievedFromApprenticeshipsApi_UpsertApprenticeshipProgrammesReferencedataThrowInfrastuctureException()
        {
            _mockStandardsClient
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new [] { _standardOne });

            _mockFrameworksClient
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(Enumerable.Empty<FrameworkSummary>());

            Func<Task> asyncFunction = async () => { await _sut.UpdateApprenticeshipProgrammesAsync(); };
            (await asyncFunction.Should().ThrowAsync<InfrastructureException>()).WithMessage("Retrieved 0 frameworks from the apprenticeships api.");
        }

        [Fact]
        public async Task WhenApprenticeshipsApiReturnsDuplicateStandards_ThenOnlyDistinctStandardsAreUpsertedIntoReferenceData()
        {
            // Arrange
            _mockStandardsClient
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(
                Enumerable
                .Repeat(_standardOne, 2)
                .Append(_standardTwo)
            );

            _mockFrameworksClient
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new [] { _frameworkOne });

            ApprenticeshipProgrammesReferenceData apprenticeshipProgrammesReferenceData = null;

            _mockReferenceDataWriter.Setup(x => x.UpsertReferenceData(It.IsAny<ApprenticeshipProgrammesReferenceData>()))
            .Callback<ApprenticeshipProgrammesReferenceData>(arg => apprenticeshipProgrammesReferenceData = arg)
            .Returns(Task.CompletedTask);

            // Act
            await _sut.UpdateApprenticeshipProgrammesAsync();

            // Assert
            var firstItem = apprenticeshipProgrammesReferenceData.Data.Where(ap => ap.ApprenticeshipType == TrainingType.Standard).First();
            var secondItem = apprenticeshipProgrammesReferenceData.Data.Where(ap => ap.ApprenticeshipType == TrainingType.Standard).Skip(1).First();

            _mockReferenceDataWriter.Verify(x => x.UpsertReferenceData(apprenticeshipProgrammesReferenceData), Times.Once);
            apprenticeshipProgrammesReferenceData.Data.Count.Should().Be(3);
            apprenticeshipProgrammesReferenceData.Data.Where(ap => ap.ApprenticeshipType == TrainingType.Standard).Count().Should().Be(2);
            firstItem.Id.Equals(secondItem.Id).Should().BeFalse();
            firstItem.ApprenticeshipLevel.Equals(secondItem.ApprenticeshipLevel).Should().BeFalse();
            firstItem.Title.Equals(secondItem.Title).Should().BeFalse();
        }

        [Fact]
        public async Task WhenApprenticeshipsApiReturnsDuplicateFrameworks_ThenOnlyDistinctFrameworksAreUpsertedIntoReferenceData()
        {
            // Arrange
            _mockStandardsClient
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new [] { _standardOne });

            _mockFrameworksClient
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(
                Enumerable
                .Repeat(_frameworkOne, 2)
                .Append(_frameworkTwo)
            );

            ApprenticeshipProgrammesReferenceData apprenticeshipProgrammesReferenceData = null;

            _mockReferenceDataWriter.Setup(x => x.UpsertReferenceData(It.IsAny<ApprenticeshipProgrammesReferenceData>()))
            .Callback<ApprenticeshipProgrammesReferenceData>(arg => apprenticeshipProgrammesReferenceData = arg)
            .Returns(Task.CompletedTask);

            // Act
            await _sut.UpdateApprenticeshipProgrammesAsync();

            // Assert
            var firstItem = apprenticeshipProgrammesReferenceData.Data.Where(ap => ap.ApprenticeshipType == TrainingType.Framework).First();
            var secondItem = apprenticeshipProgrammesReferenceData.Data.Where(ap => ap.ApprenticeshipType == TrainingType.Framework).Skip(1).First();

            _mockReferenceDataWriter.Verify(x => x.UpsertReferenceData(apprenticeshipProgrammesReferenceData), Times.Once);
            apprenticeshipProgrammesReferenceData.Data.Count().Should().Be(3);
            apprenticeshipProgrammesReferenceData.Data.Where(ap => ap.ApprenticeshipType == TrainingType.Framework).Count().Should().Be(2);
            firstItem.Id.Equals(secondItem.Id).Should().BeFalse();
            firstItem.ApprenticeshipLevel.Equals(secondItem.ApprenticeshipLevel).Should().BeFalse();
            firstItem.Title.Equals(secondItem.Title).Should().BeFalse();
        }

        [Fact]
        public async Task WhenApprenticeshipsApiReturnsDuplicateFrameworksAndStandards_ThenOnlyDistinctFrameworksAndStandardsAreUpsertedIntoReferenceData()
        {
            // Arrange
            _mockStandardsClient
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(
                Enumerable
                .Repeat(_standardOne, 2)
                .Union(Enumerable.Repeat(_standardTwo, 5))
            );

            _mockFrameworksClient
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(
                Enumerable
                .Repeat(_frameworkOne, 2)
                .Union(Enumerable.Repeat(_frameworkTwo, 11))
            );

            ApprenticeshipProgrammesReferenceData apprenticeshipProgrammesReferenceData = null;

            _mockReferenceDataWriter.Setup(x => x.UpsertReferenceData(It.IsAny<ApprenticeshipProgrammesReferenceData>()))
            .Callback<ApprenticeshipProgrammesReferenceData>(arg => apprenticeshipProgrammesReferenceData = arg)
            .Returns(Task.CompletedTask);

            // Act
            await _sut.UpdateApprenticeshipProgrammesAsync();

            _mockReferenceDataWriter.Verify(x => x.UpsertReferenceData(apprenticeshipProgrammesReferenceData), Times.Once);
            apprenticeshipProgrammesReferenceData.Data.Count.Should().Be(4);
            apprenticeshipProgrammesReferenceData.Data.Count(ap => ap.ApprenticeshipType == TrainingType.Framework).Should().Be(2);
            apprenticeshipProgrammesReferenceData.Data.Count(ap => ap.ApprenticeshipType == TrainingType.Standard).Should().Be(2);
        }        
    }
}