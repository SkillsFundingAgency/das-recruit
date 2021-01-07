using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Services.ReferenceData;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.ApprenticeshipProgrammes;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using ApprenticeshipProgrammesReferenceData = Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.ApprenticeshipProgrammes.ApprenticeshipProgrammes;

namespace Esfa.Recruit.UnitTests.Vacancies.Client.Infrastructure.ReferenceData.ApprenticeshipProgrammes
{
    [Trait("Category", "Unit")]
    public class ApprenticeshipProgrammesUpdaterServiceTests
    {
        private readonly IApprenticeshipProgrammesUpdateService _sut;
        private readonly Mock<IOuterApiClient> _mockOuterApiClient;
        private readonly Mock<IReferenceDataWriter> _mockReferenceDataWriter;

        private readonly GetTrainingProgrammesResponseItem _standardOne = new GetTrainingProgrammesResponseItem
        {
            Id = "1",
            Title = "Network engineer",
            ApprenticeshipType = TrainingType.Standard,
            Duration = 24,
            ApprenticeshipLevel = ApprenticeshipLevel.Higher,
            EffectiveFrom = new DateTime(2014, 8, 1),
            IsActive = true
        };
        private readonly GetTrainingProgrammesResponseItem _standardTwo = new GetTrainingProgrammesResponseItem
        {
            Id = "10",
            Title = "Electrical / electronic technical support engineer",
            Duration = 60,
            ApprenticeshipType = TrainingType.Standard,
            ApprenticeshipLevel = ApprenticeshipLevel.Degree,
            EffectiveFrom = new DateTime(2014, 8, 1),
            IsActive = true
        };       
        private readonly GetTrainingProgrammesResponseItem _frameworkOne = new GetTrainingProgrammesResponseItem
        {
            Id = "401-3-1",
            Title = "Drinks Dispense Systems = Drinks Dispense Systems",
            ApprenticeshipType = TrainingType.Framework,
            ApprenticeshipLevel = ApprenticeshipLevel.Intermediate,
            Duration = 0,
            EffectiveFrom = new DateTime(2013, 2, 1),
            EffectiveTo = new DateTime(2013, 7, 31),
            IsActive = false
        };
        private readonly GetTrainingProgrammesResponseItem _frameworkTwo = new GetTrainingProgrammesResponseItem
        {
            Id = "403-2-1",
            Title = "Food and Drink = Meat and Poultry Industry Skills",
            ApprenticeshipType = TrainingType.Framework,
            ApprenticeshipLevel = ApprenticeshipLevel.Advanced,
            Duration = 12,
            EffectiveFrom = new DateTime(2013, 2, 1),
            EffectiveTo = new DateTime(2018, 4, 30),
            IsActive = false,
        };            

        public ApprenticeshipProgrammesUpdaterServiceTests()
        {
            var mockReferenceDataReader = new Mock<IReferenceDataReader>();
            _mockOuterApiClient = new Mock<IOuterApiClient>();
            _mockReferenceDataWriter = new Mock<IReferenceDataWriter>();

            _sut = new ApprenticeshipProgrammesUpdateService(Mock.Of<ILogger<ApprenticeshipProgrammesUpdateService>>(),
                                                            _mockReferenceDataWriter.Object,
                                                            mockReferenceDataReader.Object,
                                                            _mockOuterApiClient.Object);
        }

        [Fact]
        public async Task WhenZeroStandardsAreRetrievedFromApprenticeshipsApi_UpsertApprenticeshipProgrammesReferencedataThrowInfrastuctureException()
        {
            _mockOuterApiClient
                .Setup(x => x.Get<GetTrainingProgrammesResponse>(It.IsAny<GetTrainingProgrammesRequest>()))
                .ReturnsAsync(new GetTrainingProgrammesResponse
                {
                    TrainingProgrammes = new List<GetTrainingProgrammesResponseItem> {_frameworkOne}
                });


            Func<Task> asyncFunction = async () => { await _sut.UpdateApprenticeshipProgrammesAsync(); };
            (await asyncFunction.Should().ThrowAsync<InfrastructureException>()).WithMessage("Retrieved 0 standards from the apprenticeships api.");
        }

        [Fact]
        public async Task WhenZeroFrameworksAreRetrievedFromApprenticeshipsApi_UpsertApprenticeshipProgrammesReferencedataThrowInfrastuctureException()
        {
            _mockOuterApiClient
                .Setup(x => x.Get<GetTrainingProgrammesResponse>(It.IsAny<GetTrainingProgrammesRequest>()))
                .ReturnsAsync(new GetTrainingProgrammesResponse
                {
                    TrainingProgrammes = new List<GetTrainingProgrammesResponseItem> {_standardOne}
                });
            
            Func<Task> asyncFunction = async () => { await _sut.UpdateApprenticeshipProgrammesAsync(); };
            (await asyncFunction.Should().ThrowAsync<InfrastructureException>()).WithMessage("Retrieved 0 frameworks from the apprenticeships api.");
        }

        [Fact]
        public async Task WhenApprenticeshipsApiReturnsDuplicateStandards_ThenOnlyDistinctStandardsAreUpsertedIntoReferenceData()
        {
            // Arrange
            _mockOuterApiClient
                .Setup(x => x.Get<GetTrainingProgrammesResponse>(It.IsAny<GetTrainingProgrammesRequest>()))
                .ReturnsAsync(new GetTrainingProgrammesResponse
                {
                    TrainingProgrammes = new List<GetTrainingProgrammesResponseItem> {_standardOne, _standardOne, _standardTwo, _frameworkOne}
                });
            
            ApprenticeshipProgrammesReferenceData apprenticeshipProgrammesReferenceData = null;

            _mockReferenceDataWriter.Setup(x => x.UpsertReferenceData(It.IsAny<ApprenticeshipProgrammesReferenceData>()))
            .Callback<ApprenticeshipProgrammesReferenceData>(arg => apprenticeshipProgrammesReferenceData = arg)
            .Returns(Task.CompletedTask);

            // Act
            await _sut.UpdateApprenticeshipProgrammesAsync();

            // Assert
            var firstItem = apprenticeshipProgrammesReferenceData.Data.First(ap => ap.ApprenticeshipType == TrainingType.Standard);
            var secondItem = apprenticeshipProgrammesReferenceData.Data.Where(ap => ap.ApprenticeshipType == TrainingType.Standard).Skip(1).First();

            _mockReferenceDataWriter.Verify(x => x.UpsertReferenceData(apprenticeshipProgrammesReferenceData), Times.Once);
            apprenticeshipProgrammesReferenceData.Data.Count.Should().Be(3);
            apprenticeshipProgrammesReferenceData.Data.Count(ap => ap.ApprenticeshipType == TrainingType.Standard).Should().Be(2);
            firstItem.Id.Equals(secondItem.Id).Should().BeFalse();
            firstItem.ApprenticeshipLevel.Equals(secondItem.ApprenticeshipLevel).Should().BeFalse();
            firstItem.Title.Equals(secondItem.Title).Should().BeFalse();
        }

        [Fact]
        public async Task WhenApprenticeshipsApiReturnsDuplicateFrameworks_ThenOnlyDistinctFrameworksAreUpsertedIntoReferenceData()
        {
            // Arrange
            _mockOuterApiClient
                .Setup(x => x.Get<GetTrainingProgrammesResponse>(It.IsAny<GetTrainingProgrammesRequest>()))
                .ReturnsAsync(new GetTrainingProgrammesResponse
                {
                    TrainingProgrammes = new List<GetTrainingProgrammesResponseItem> {_standardOne, _frameworkOne, _frameworkOne, _frameworkTwo}
                });
            
            ApprenticeshipProgrammesReferenceData apprenticeshipProgrammesReferenceData = null;

            _mockReferenceDataWriter.Setup(x => x.UpsertReferenceData(It.IsAny<ApprenticeshipProgrammesReferenceData>()))
            .Callback<ApprenticeshipProgrammesReferenceData>(arg => apprenticeshipProgrammesReferenceData = arg)
            .Returns(Task.CompletedTask);

            // Act
            await _sut.UpdateApprenticeshipProgrammesAsync();

            // Assert
            var firstItem = apprenticeshipProgrammesReferenceData.Data.First(ap => ap.ApprenticeshipType == TrainingType.Framework);
            var secondItem = apprenticeshipProgrammesReferenceData.Data.Where(ap => ap.ApprenticeshipType == TrainingType.Framework).Skip(1).First();

            _mockReferenceDataWriter.Verify(x => x.UpsertReferenceData(apprenticeshipProgrammesReferenceData), Times.Once);
            apprenticeshipProgrammesReferenceData.Data.Count().Should().Be(3);
            apprenticeshipProgrammesReferenceData.Data.Count(ap => ap.ApprenticeshipType == TrainingType.Framework).Should().Be(2);
            firstItem.Id.Equals(secondItem.Id).Should().BeFalse();
            firstItem.ApprenticeshipLevel.Equals(secondItem.ApprenticeshipLevel).Should().BeFalse();
            firstItem.Title.Equals(secondItem.Title).Should().BeFalse();
        }

        [Fact]
        public async Task WhenApprenticeshipsApiReturnsDuplicateFrameworksAndStandards_ThenOnlyDistinctFrameworksAndStandardsAreUpsertedIntoReferenceData()
        {
            // Arrange
            _mockOuterApiClient
                .Setup(x => x.Get<GetTrainingProgrammesResponse>(It.IsAny<GetTrainingProgrammesRequest>()))
                .ReturnsAsync(new GetTrainingProgrammesResponse
                {
                    TrainingProgrammes = new List<GetTrainingProgrammesResponseItem> {_standardOne,_standardTwo, _standardTwo, _standardTwo, _frameworkOne, _frameworkTwo, _frameworkTwo, _frameworkTwo, _frameworkTwo}
                });
            
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