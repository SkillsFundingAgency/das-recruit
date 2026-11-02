using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.TrainingProviders;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.ReferenceData.TrainingProviders;

public class TrainingProviderUpdateServiceTests
{
    private readonly Mock<IOuterApiClient> _mockOuterApiClient;
    private readonly Mock<IReferenceDataWriter> _mockReferenceDataWriter;
    private readonly TrainingProvidersUpdateService _sut;

    public TrainingProviderUpdateServiceTests ()
    {
        _mockOuterApiClient = new Mock<IOuterApiClient>();
        _mockReferenceDataWriter = new Mock<IReferenceDataWriter>();

        _sut = new TrainingProvidersUpdateService(Mock.Of<ILogger<TrainingProvidersUpdateService>>(),
            _mockReferenceDataWriter.Object,
            _mockOuterApiClient.Object);
    }

    [Theory]
    [InlineData(ProviderTypeIdentifier.MainProvider)]
    [InlineData(ProviderTypeIdentifier.EmployerProvider)]
    public async Task Then_The_Data_Is_Taken_From_The_Outer_Api_And_Upserted_For_Main_And_Employer_Profile(ProviderTypeIdentifier providerType)
    {
        // Arrange
        var fixture = new Fixture();
        fixture.Customize<GetProviderResponseItem>(c => 
            c.With(x => x.ProviderTypeId, (short)providerType)
                .With(x => x.StatusId, (short)ProviderStatusType.Active));
        var providerResponse = fixture.Create<GetProvidersResponse>();
        _mockOuterApiClient
            .Setup(x => x.Get<GetProvidersResponse>(It.IsAny<GetProvidersRequest>()))
            .ReturnsAsync(providerResponse);

        Recruit.Vacancies.Client.Infrastructure.ReferenceData.TrainingProviders.TrainingProviders trainingProviders = null;

        _mockReferenceDataWriter.Setup(x => x.UpsertReferenceData(It.IsAny<Recruit.Vacancies.Client.Infrastructure.ReferenceData.TrainingProviders.TrainingProviders>()))
            .Callback<Recruit.Vacancies.Client.Infrastructure.ReferenceData.TrainingProviders.TrainingProviders>(arg => trainingProviders = arg)
            .Returns(Task.CompletedTask);

        //Act
        await _sut.UpdateProviders();

        //Assert
        _mockReferenceDataWriter.Verify(x => x.UpsertReferenceData(trainingProviders), Times.Once);
        trainingProviders.Data.Should()
            .BeEquivalentTo(providerResponse.Providers.Select(c => (TrainingProvider)c).ToList());
    }

    [Theory]
    [InlineData(ProviderTypeIdentifier.SupportingProvider)]
    [InlineData(ProviderTypeIdentifier.EPAO)]
    public async Task Then_The_Data_Is_Taken_From_The_Outer_Api_And_Upserted_For_Non_Main_And_Employer_Profile_Providers(ProviderTypeIdentifier providerType)
    {
        // Arrange
        var fixture = new Fixture();
        fixture.Customize<GetProviderResponseItem>(c => c.With(x => x.ProviderTypeId, (short)providerType));
        var providerResponse = fixture.Create<GetProvidersResponse>();
        _mockOuterApiClient
            .Setup(x => x.Get<GetProvidersResponse>(It.IsAny<GetProvidersRequest>()))
            .ReturnsAsync(providerResponse);

        Recruit.Vacancies.Client.Infrastructure.ReferenceData.TrainingProviders.TrainingProviders trainingProviders = null;

        _mockReferenceDataWriter.Setup(x => x.UpsertReferenceData(It.IsAny<Recruit.Vacancies.Client.Infrastructure.ReferenceData.TrainingProviders.TrainingProviders>()))
            .Callback<Recruit.Vacancies.Client.Infrastructure.ReferenceData.TrainingProviders.TrainingProviders>(arg => trainingProviders = arg)
            .Returns(Task.CompletedTask);

        //Act
        await _sut.UpdateProviders();

        //Assert
        _mockReferenceDataWriter.Verify(x => x.UpsertReferenceData(trainingProviders), Times.Once);
        trainingProviders.Data.Count.Should().Be(0);
    }

    [Theory]
    [InlineData(ProviderTypeIdentifier.MainProvider)]
    [InlineData(ProviderTypeIdentifier.EmployerProvider)]
    public async Task Then_The_Data_Is_Taken_From_The_Outer_Api_And_Upserted_For_Main_And_Employer_Profile_Providers_With_Status_ActiveButNotTakingApprentices(ProviderTypeIdentifier providerType)
    {
        // Arrange
        var fixture = new Fixture();
        fixture.Customize<GetProviderResponseItem>(c =>
            c.With(x => x.ProviderTypeId, (short)providerType)
                .With(x => x.StatusId, (short)ProviderStatusType.ActiveButNotTakingOnApprentices));
        var providerResponse = fixture.Create<GetProvidersResponse>();
        _mockOuterApiClient
            .Setup(x => x.Get<GetProvidersResponse>(It.IsAny<GetProvidersRequest>()))
            .ReturnsAsync(providerResponse);

        Recruit.Vacancies.Client.Infrastructure.ReferenceData.TrainingProviders.TrainingProviders trainingProviders = null;

        _mockReferenceDataWriter.Setup(x => x.UpsertReferenceData(It.IsAny<Recruit.Vacancies.Client.Infrastructure.ReferenceData.TrainingProviders.TrainingProviders>()))
            .Callback<Recruit.Vacancies.Client.Infrastructure.ReferenceData.TrainingProviders.TrainingProviders>(arg => trainingProviders = arg)
            .Returns(Task.CompletedTask);

        //Act
        await _sut.UpdateProviders();

        //Assert
        _mockReferenceDataWriter.Verify(x => x.UpsertReferenceData(trainingProviders), Times.Once);
        trainingProviders.Data.Count.Should().Be(0);
    }
}