﻿using AutoFixture.NUnit3;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Controllers.Part1;
using Esfa.Recruit.Employer.Web.Models.AddLocation;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.AddLocation;
using Esfa.Recruit.Shared.Web.Domain;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Locations;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Controllers.Part1;

public class AddLocationControllerTests
{
    [Test, MoqAutoData]
    public async Task When_Getting_AddLocation_Then_ViewModel_Is_Returned(
        [Frozen] Vacancy vacancy,
        [Greedy] AddLocationController sut)
    {
        // arrange
        var addLocationModel = new AddLocationJourneyModel()
        {
            VacancyId = vacancy.Id,
            EmployerAccountId = vacancy.EmployerAccountId,
            Origin = MultipleLocationsJourneyOrigin.Many,
            Wizard = true
        };
        sut.AddControllerContext().WithTempData();
        
        // act
        var result = (await sut.AddLocation(addLocationModel) as ViewResult)?.Model as AddLocationViewModel;
        
        // assert
        result.Should().NotBeNull();
        result!.VacancyId.Should().Be(vacancy.Id);
        result.ApprenticeshipTitle.Should().Be(vacancy.Title);
        result.EmployerAccountId.Should().BeEquivalentTo(vacancy.EmployerAccountId);
        result.Origin.Should().Be(MultipleLocationsJourneyOrigin.Many);
        result.Postcode.Should().Be(null);
    }

    [Test, MoqAutoData]
    public async Task When_Posting_To_AddLocation_With_Valid_Request_Then_Redirect_To_Select_An_Address(
        AddLocationEditModel addLocationModel,
        GetAddressesListResponse getAddressesListResponse,
        [Frozen] Mock<ILocationsService> locationsService,
        [Frozen] Mock<IGetAddressesClient> getAddressesClient,
        [Greedy] AddLocationController sut)
    {
        // arrange
        locationsService.Setup(x => x.IsPostcodeInEnglandAsync(addLocationModel.Postcode)).ReturnsAsync(true);
        getAddressesClient.Setup(x => x.GetAddresses(It.IsAny<string>())).ReturnsAsync(getAddressesListResponse);
        sut.AddControllerContext().WithTempData();
        
        // act
        var result = await sut.AddLocation(locationsService.Object, getAddressesClient.Object, addLocationModel) as RedirectToRouteResult;
        
        // assert
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.SelectAnAddress_Get);
    }
    
    [Test, MoqAutoData]
    public async Task When_Posting_To_AddLocation_With_Invalid_Request_Then_Return_View(
        AddLocationEditModel addLocationEditModel,
        [Frozen] Mock<IGetAddressesClient> getAddressesClient,
        [Frozen] Vacancy vacancy,
        [Frozen] Mock<ILocationsService> locationsService,
        [Greedy] AddLocationController sut)
    {
        // arrange
        addLocationEditModel.Origin = MultipleLocationsJourneyOrigin.Many;
        getAddressesClient.Setup(x => x.GetAddresses(It.IsAny<string>())).ReturnsAsync((GetAddressesListResponse)null);
        locationsService.Setup(x => x.IsPostcodeInEnglandAsync(addLocationEditModel.Postcode)).ReturnsAsync(false);
        
        // act
        var result = (await sut.AddLocation(locationsService.Object, getAddressesClient.Object, addLocationEditModel) as ViewResult)?.Model as AddLocationViewModel;
        
        // assert
        result.Should().NotBeNull();
        result!.VacancyId.Should().Be(addLocationEditModel.VacancyId);
        result.ApprenticeshipTitle.Should().Be(vacancy.Title);
        result.EmployerAccountId.Should().BeEquivalentTo(addLocationEditModel.EmployerAccountId);
        result.Origin.Should().Be(addLocationEditModel.Origin);
        result.Postcode.Should().Be(addLocationEditModel.Postcode);
    }
}