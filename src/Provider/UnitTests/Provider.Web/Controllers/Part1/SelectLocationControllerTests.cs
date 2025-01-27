using System.Linq;
using AutoFixture.NUnit3;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Controllers.Part1;
using Esfa.Recruit.Provider.Web.Models.AddLocation;
using Esfa.Recruit.Provider.Web.Services;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.AddLocation;
using Esfa.Recruit.Shared.Web.Domain;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Controllers.Part1;

public class SelectLocationControllerTests
{
    private const string Postcode = "XXXXXXX";
    
    [Test, MoqAutoData]
    public async Task When_Getting_SelectLocation_With_Postcode_View_Is_Returned(
        [Frozen] Vacancy vacancy,
        [Frozen] IGetAddressesClient getAddressesClient,
        [Greedy] SelectLocationController sut)
    {
        // arrange
        sut.AddControllerContext().WithTempData();
        sut.TempData.Add(TempDataKeys.Postcode, Postcode);
        var model = new AddLocationJourneyModel
        {
            VacancyId = vacancy.Id,
            Ukprn = new Random().Next(),
            Origin = MultipleLocationsJourneyOrigin.Many,
            Wizard = true
        };
        
        // act
        var result = (await sut.SelectLocation(getAddressesClient, model) as ViewResult)?.Model as SelectLocationViewModel;
        
        // assert
        result.Should().NotBeNull();
        result!.VacancyId.Should().Be(vacancy.Id);
        result.ApprenticeshipTitle.Should().Be(vacancy.Title);
        result.Ukprn.Should().Be(model.Ukprn);
        result.Origin.Should().Be(MultipleLocationsJourneyOrigin.Many);
        result.Postcode.Should().Be(Postcode);
    }
    
    [Test, MoqAutoData]
    public async Task When_Getting_SelectLocation_View_Without_Postcode_RedirectToRouteResult_Is_Returned(
        [Frozen] Vacancy vacancy,
        [Frozen] IGetAddressesClient getAddressesClient,
        [Greedy] SelectLocationController sut)
    {
        // arrange
        sut.AddControllerContext().WithTempData();
        var model = new AddLocationJourneyModel
        {
            VacancyId = vacancy.Id,
            Ukprn = new Random().Next(),
            Origin = MultipleLocationsJourneyOrigin.Many,
            Wizard = true
        };
        
        // act
        var result = await sut.SelectLocation(getAddressesClient, model) as RedirectToRouteResult;
        
        // assert
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.AddLocation_Get);
    }
    
    [Test, MoqAutoData]
    public async Task When_Posting_SelectLocation_View_Without_Postcode_RedirectToRouteResult_Is_Returned(
        [Frozen] Vacancy vacancy,
        IVacancyLocationService vacancyLocationService,
        IGetAddressesClient getAddressesClient,
        IRecruitVacancyClient recruitVacancyClient,
        [Greedy] SelectLocationController sut)
    {
        // arrange
        sut.AddControllerContext().WithTempData();
        var model = new SelectLocationEditModel
        {
            VacancyId = vacancy.Id,
            Ukprn = new Random().Next(),
            Origin = MultipleLocationsJourneyOrigin.Many,
            Wizard = true
        };
        
        // act
        var result = await sut.SelectLocation(vacancyLocationService, recruitVacancyClient, getAddressesClient, model) as RedirectToRouteResult;
        
        // assert
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.AddLocation_Get);
    }
    
    [Test, MoqAutoData]
    public async Task When_Posting_SelectLocation_View_And_Address_Cannot_Be_Found_View_Is_Returned(
        [Frozen] Vacancy vacancy,
        IVacancyLocationService vacancyLocationService,
        Mock<IGetAddressesClient> getAddressesClient,
        IRecruitVacancyClient recruitVacancyClient,
        [Greedy] SelectLocationController sut)
    {
        // arrange
        sut.AddControllerContext().WithTempData();
        sut.TempData.Add(TempDataKeys.Postcode, Postcode);
        var model = new SelectLocationEditModel
        {
            VacancyId = vacancy.Id,
            Ukprn = new Random().Next(),
            Origin = MultipleLocationsJourneyOrigin.Many,
            Wizard = true
        };
        getAddressesClient.Setup(x => x.GetAddresses(It.IsAny<string>())).ReturnsAsync((GetAddressesListResponse)null);
        
        // act
        var result = (await sut.SelectLocation(vacancyLocationService, recruitVacancyClient, getAddressesClient.Object, model) as ViewResult)?.Model as SelectLocationViewModel;
        
        // assert
        result.Should().NotBeNull();
        result!.VacancyId.Should().Be(vacancy.Id);
        result.ApprenticeshipTitle.Should().Be(vacancy.Title);
        result.Ukprn.Should().Be(model.Ukprn);
        result.Origin.Should().Be(MultipleLocationsJourneyOrigin.Many);
        result.Postcode.Should().Be(Postcode);
    }
    
    [Test, MoqAutoData]
    public async Task When_Posting_SelectLocation_View_With_Valid_Selected_Address_Redirects_To_AddManyLocations_Route(
        [Frozen] GetAddressesListResponse getAddressesListResponse,
        [Frozen] Vacancy vacancy,
        IVacancyLocationService vacancyLocationService,
        Mock<IGetAddressesClient> getAddressesClient,
        Mock<IRecruitVacancyClient> recruitVacancyClient,
        [Greedy] SelectLocationController sut)
    {
        // arrange
        int ukprn = new Random().Next();
        sut
            .AddControllerContext()
            .WithTempData()
            .WithUser(Guid.NewGuid())
            .WithClaim(ProviderRecruitClaims.IdamsUserUkprnClaimsTypeIdentifier, ukprn.ToString());
        sut.TempData.Add(TempDataKeys.Postcode, Postcode);
        var model = new SelectLocationEditModel
        {
            Ukprn = ukprn,
            Origin = MultipleLocationsJourneyOrigin.Many,
            SelectedLocation = getAddressesListResponse.Addresses.First().ToShortAddress(),
            VacancyId = vacancy.Id,
            Wizard = true
        };
        
        
        // act
        var result = (await sut.SelectLocation(vacancyLocationService, recruitVacancyClient.Object, getAddressesClient.Object, model) as RedirectToRouteResult);
        
        // assert
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.AddMoreThanOneLocation_Get);
        sut.TempData.Keys.Should().NotContain(TempDataKeys.Postcode);
        (sut.TempData[TempDataKeys.AddedLocation] as string).Should().StartWith(model.SelectedLocation);
        recruitVacancyClient.Verify(x => x.UpdateEmployerProfileAsync(It.IsAny<EmployerProfile>(), It.IsAny<VacancyUser>()), Times.Once);
    }
    
    [Test, MoqAutoData]
    public async Task When_Posting_SelectLocation_View_With_Valid_Duplicate_Selected_Address_Redirects_To_AddManyLocations_Route(
        [Frozen] GetAddressesListResponse getAddressesListResponse,
        [Frozen] Vacancy vacancy,
        Mock<IVacancyLocationService> vacancyLocationService,
        Mock<IGetAddressesClient> getAddressesClient,
        Mock<IRecruitVacancyClient> recruitVacancyClient,
        [Greedy] SelectLocationController sut)
    {
        // arrange
        int ukprn = new Random().Next();
        sut
            .AddControllerContext()
            .WithTempData()
            .WithUser(Guid.NewGuid())
            .WithClaim(ProviderRecruitClaims.IdamsUserUkprnClaimsTypeIdentifier, ukprn.ToString());
        sut.TempData.Add(TempDataKeys.Postcode, Postcode);
        var firstAddress = getAddressesListResponse.Addresses.First();
        firstAddress.Postcode = Postcode;
        var model = new SelectLocationEditModel
        {
            Ukprn = ukprn,
            Origin = MultipleLocationsJourneyOrigin.Many,
            SelectedLocation = firstAddress.ToShortAddress(),
            VacancyId = vacancy.Id,
            Wizard = true
        };
        vacancyLocationService.Setup(x => x.GetVacancyLocations(vacancy, ukprn)).ReturnsAsync([firstAddress.ToDomain()]);
        getAddressesClient.Setup(x => x.GetAddresses(firstAddress.Postcode)).ReturnsAsync(new GetAddressesListResponse { Addresses = [firstAddress] });
        
        // act
        var result = (await sut.SelectLocation(vacancyLocationService.Object, recruitVacancyClient.Object, getAddressesClient.Object, model) as RedirectToRouteResult);
        
        // assert
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.AddMoreThanOneLocation_Get);
        sut.TempData.Keys.Should().NotContain(TempDataKeys.Postcode);
        (sut.TempData[TempDataKeys.AddedLocation] as string).Should().StartWith(model.SelectedLocation);
        recruitVacancyClient.Verify(x => x.UpdateEmployerProfileAsync(It.IsAny<EmployerProfile>(), It.IsAny<VacancyUser>()), Times.Never);
    }
}