using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Locations;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.Services;

public class LocationsServiceTests
{
    private const string Postcode = "CV1 2WT";
    
    [Test, MoqAutoData]
    public async Task IsPostcodeInEnglandAsync_Should_Return_False_If_Postcode_Location_Is_Unknown(
        [Frozen] Mock<ILocationsClient> locationsClient,
        [Greedy] LocationsService sut)
    {
        // arrange
        locationsClient
            .Setup(x => x.GetPostcodeData(Postcode))
            .ReturnsAsync(new GetPostcodeDataResponse(Postcode, null));

        // act
        var result = await sut.IsPostcodeInEnglandAsync(Postcode);

        // assert
        result.Should().BeFalse();
    }
    
    [Test, MoqAutoData]
    public async Task IsPostcodeInEnglandAsync_Should_Return_False_If_No_Api_Result(
        [Frozen] Mock<ILocationsClient> locationsClient,
        [Greedy] LocationsService sut)
    {
        // arrange
        locationsClient
            .Setup(x => x.GetPostcodeData(Postcode))
            .ReturnsAsync(null as GetPostcodeDataResponse);

        // act
        var result = await sut.IsPostcodeInEnglandAsync(Postcode);

        // assert
        result.Should().BeFalse();
    }
    
    [Test]
    [MoqInlineAutoData("England", true)]
    [MoqInlineAutoData("Scotland", false)]
    [MoqInlineAutoData("Wales", false)]
    [MoqInlineAutoData("Northern Ireland", false)]
    [MoqInlineAutoData("France", false)]
    public async Task IsPostcodeInEnglandAsync_Should_Return_Correct_Result_If_Postcode_Is_In(
        string country,
        bool expectedResult,
        [Frozen] Mock<ILocationsClient> locationsClient,
        [Greedy] LocationsService sut)
    {
        // arrange
        locationsClient
            .Setup(x => x.GetPostcodeData(Postcode))
            .ReturnsAsync(new GetPostcodeDataResponse(Postcode, new PostcodeData(Postcode, country, 1, 1)));

        // act
        var result = await sut.IsPostcodeInEnglandAsync(Postcode);

        // assert
        result.Should().Be(expectedResult);
    }
}