using System.Linq;
using AutoFixture.NUnit3;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Controllers.Part1;
using Esfa.Recruit.Employer.Web.Models.AddLocation;
using Esfa.Recruit.Employer.Web.Services;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.AddLocation;
using Esfa.Recruit.Shared.Web.Domain;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Controllers.Part1;

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
            EmployerAccountId = vacancy.EmployerAccountId,
            Origin = MultipleLocationsJourneyOrigin.Many,
            Wizard = true
        };
        
        // act
        var result = (await sut.SelectLocation(getAddressesClient, model) as ViewResult)?.Model as SelectLocationViewModel;
        
        // assert
        result.Should().NotBeNull();
        result!.VacancyId.Should().Be(vacancy.Id);
        result.ApprenticeshipTitle.Should().Be(vacancy.Title);
        result.EmployerAccountId.Should().BeEquivalentTo(vacancy.EmployerAccountId);
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
            EmployerAccountId = vacancy.EmployerAccountId,
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
        [Greedy] SelectLocationController sut)
    {
        // arrange
        sut.AddControllerContext().WithTempData();
        var model = new SelectLocationEditModel
        {
            VacancyId = vacancy.Id,
            EmployerAccountId = vacancy.EmployerAccountId,
            Origin = MultipleLocationsJourneyOrigin.Many,
            Wizard = true
        };
        
        // act
        var result = await sut.SelectLocation(vacancyLocationService, getAddressesClient, model) as RedirectToRouteResult;
        
        // assert
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.AddLocation_Get);
    }
    
    [Test, MoqAutoData]
    public async Task When_Posting_SelectLocation_View_And_Address_Cannot_Be_Found_View_Is_Returned(
        [Frozen] Vacancy vacancy,
        IVacancyLocationService vacancyLocationService,
        Mock<IGetAddressesClient> getAddressesClient,
        
        [Greedy] SelectLocationController sut)
    {
        // arrange
        sut.AddControllerContext().WithTempData();
        sut.TempData.Add(TempDataKeys.Postcode, Postcode);
        var model = new SelectLocationEditModel
        {
            VacancyId = vacancy.Id,
            EmployerAccountId = vacancy.EmployerAccountId,
            Origin = MultipleLocationsJourneyOrigin.Many,
            Wizard = true
        };
        getAddressesClient.Setup(x => x.GetAddresses(It.IsAny<string>())).ReturnsAsync((GetAddressesListResponse)null);
        
        // act
        var result = (await sut.SelectLocation(vacancyLocationService, getAddressesClient.Object, model) as ViewResult)?.Model as SelectLocationViewModel;
        
        // assert
        result.Should().NotBeNull();
        result!.VacancyId.Should().Be(vacancy.Id);
        result.ApprenticeshipTitle.Should().Be(vacancy.Title);
        result.EmployerAccountId.Should().BeEquivalentTo(vacancy.EmployerAccountId);
        result.Origin.Should().Be(MultipleLocationsJourneyOrigin.Many);
        result.Postcode.Should().Be(Postcode);
    }
    
    [Test, MoqAutoData]
    public async Task When_Posting_SelectLocation_View_With_Valid_Selected_Address_Redirects_To_AddManyLocations_Route(
        [Frozen] GetAddressesListResponse getAddressesListResponse,
        [Frozen] Vacancy vacancy,
        Mock<IVacancyLocationService> vacancyLocationService,
        Mock<IGetAddressesClient> getAddressesClient,
        [Greedy] SelectLocationController sut)
    {
        // arrange
        sut.AddControllerContext().WithTempData();
        sut.TempData.Add(TempDataKeys.Postcode, Postcode);
        var model = new SelectLocationEditModel
        {
            EmployerAccountId = vacancy.EmployerAccountId,
            Origin = MultipleLocationsJourneyOrigin.Many,
            SelectedLocation = getAddressesListResponse.Addresses.First().ToShortAddress(),
            VacancyId = vacancy.Id,
            Wizard = true
        };
        
        // act
        var result = (await sut.SelectLocation(vacancyLocationService.Object, getAddressesClient.Object, model) as RedirectToRouteResult);
        
        // assert
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.AddMoreThanOneLocation_Get);
        sut.TempData.Keys.Should().NotContain(TempDataKeys.Postcode);
        (sut.TempData[TempDataKeys.AddedLocation] as string).Should().StartWith(model.SelectedLocation);
        vacancyLocationService.Verify(x => x.SaveEmployerAddress(It.IsAny<VacancyUser>(), vacancy, It.IsAny<Address>()), Times.Once);
    }
    
    [Test, MoqAutoData]
    public async Task When_Posting_SelectLocation_View_With_Valid_Duplicate_Selected_Address_Redirects_To_AddManyLocations_Route(
        [Frozen] GetAddressesListResponse getAddressesListResponse,
        [Frozen] Vacancy vacancy,
        Mock<IVacancyLocationService> vacancyLocationService,
        Mock<IGetAddressesClient> getAddressesClient,
        [Greedy] SelectLocationController sut)
    {
        // arrange
        Guid userId = Guid.NewGuid();
        sut
            .AddControllerContext()
            .WithTempData()
            .WithUser(userId);
        sut.TempData.Add(TempDataKeys.Postcode, Postcode);
        var firstAddress = getAddressesListResponse.Addresses.First();
        var model = new SelectLocationEditModel
        {
            EmployerAccountId = vacancy.EmployerAccountId,
            Origin = MultipleLocationsJourneyOrigin.Many,
            SelectedLocation = firstAddress.ToShortAddress(),
            VacancyId = vacancy.Id,
            Wizard = true
        };
        vacancyLocationService.Setup(x => x.GetVacancyLocations(vacancy)).ReturnsAsync([firstAddress.ToDomain()]);
        getAddressesClient.Setup(x => x.GetAddresses(firstAddress.Postcode)).ReturnsAsync(new GetAddressesListResponse { Addresses = [firstAddress] });
        
        // act
        var result = (await sut.SelectLocation(vacancyLocationService.Object, getAddressesClient.Object, model) as RedirectToRouteResult);
        
        // assert
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.AddMoreThanOneLocation_Get);
        sut.TempData.Keys.Should().NotContain(TempDataKeys.Postcode);
        (sut.TempData[TempDataKeys.AddedLocation] as string).Should().StartWith(model.SelectedLocation);
        vacancyLocationService.Verify(x => x.SaveEmployerAddress(
                It.Is<VacancyUser>(user => user.UserId == userId.ToString()),
                vacancy,
                It.Is<Address>(address => address.Postcode == firstAddress.Postcode)),
            Times.Once
        );
    }
}