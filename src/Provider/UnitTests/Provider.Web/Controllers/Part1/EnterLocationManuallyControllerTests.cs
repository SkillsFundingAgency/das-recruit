﻿using AutoFixture.NUnit3;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Controllers.Part1;
using Esfa.Recruit.Provider.Web.Models.AddLocation;
using Esfa.Recruit.Provider.Web.Services;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.AddLocation;
using Esfa.Recruit.Shared.Web.Domain;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Locations;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Controllers.Part1;

public class EnterLocationManuallyControllerTests
{
    private const string Postcode = "XXXXXXX";
    private const string ReturnRoute = "Return_Route";
    
    [Test, MoqAutoData]
    public async Task When_Getting_SelectLocation_With_Postcode_View_Is_Returned(
        [Frozen] Vacancy vacancy,
        [Greedy] EnterLocationManuallyController sut)
    {
        // arrange
        sut.AddControllerContext().WithTempData();
        sut.TempData.Add(TempDataKeys.Postcode, Postcode);
        sut.TempData.Add(TempDataKeys.AddLocationReturnPath, ReturnRoute);
        var model = new AddLocationJourneyModel
        {
            VacancyId = vacancy.Id,
            Ukprn = new Random().Next(),
            Origin = MultipleLocationsJourneyOrigin.Many,
            Wizard = true
        };
        
        // act
        var result = (await sut.EnterLocationManually(model) as ViewResult)?.Model as EnterLocationManuallyViewModel;
        
        // assert
        result.Should().NotBeNull().And.BeEquivalentTo(model);
        result!.ApprenticeshipTitle.Should().Be(vacancy.Title);
        result.ReturnRoute.Should().Be(ReturnRoute);
        result.Postcode.Should().Be(Postcode);
    }
    
    [Test, MoqAutoData]
    public async Task When_Postcode_Is_Definitely_Not_In_England_View_Is_Returned(
        IVacancyLocationService vacancyLocationService,
        [Frozen] Mock<ILocationsService> locationsService,
        [Frozen] Vacancy vacancy,
        [Greedy] EnterLocationManuallyController sut)
    {
        // arrange
        sut.AddControllerContext().WithTempData();
        sut.TempData.Add(TempDataKeys.AddLocationReturnPath, ReturnRoute);
        var model = new EnterLocationManuallyEditModel
        {
            VacancyId = vacancy.Id,
            Ukprn = new Random().Next(),
            Origin = MultipleLocationsJourneyOrigin.Many,
            Wizard = true,
            AddressLine1 = "Address Line 1",
            AddressLine2 = "Address Line 2",
            City = "City",
            Postcode = Postcode
        };
        locationsService.Setup(x => x.IsPostcodeInEnglandAsync(Postcode)).ReturnsAsync(false);
        
        // act
        var result = (await sut.EnterLocationManually(locationsService.Object, vacancyLocationService, model) as ViewResult)?.Model as EnterLocationManuallyViewModel;
        
        // assert
        result.Should().NotBeNull().And.BeEquivalentTo(model);
        result!.ApprenticeshipTitle.Should().Be(vacancy.Title);
        result.ReturnRoute.Should().Be(ReturnRoute);
        result.Postcode.Should().Be(Postcode);
    }
    
    [Test, MoqAutoData]
    public async Task When_Postcode_Is_Unknown_Redirects_To_AddManyLocations_Route(
        IVacancyLocationService vacancyLocationService,
        [Frozen] Mock<ILocationsService> locationsService,
        [Frozen] Vacancy vacancy,
        [Greedy] EnterLocationManuallyController sut)
    {
        // arrange
        int ukprn = new Random().Next();
        sut.AddControllerContext()
            .WithTempData()
            .WithUser(Guid.NewGuid())
            .WithClaim(ProviderRecruitClaims.IdamsUserUkprnClaimsTypeIdentifier, ukprn.ToString());
        sut.TempData.Add(TempDataKeys.AddLocationReturnPath, ReturnRoute);
        var model = new EnterLocationManuallyEditModel
        {
            VacancyId = vacancy.Id,
            Ukprn = ukprn,
            Origin = MultipleLocationsJourneyOrigin.Many,
            Wizard = true,
            AddressLine1 = "Address Line 1",
            AddressLine2 = "Address Line 2",
            City = "City",
            Postcode = Postcode
        };
        locationsService.Setup(x => x.IsPostcodeInEnglandAsync(Postcode)).ReturnsAsync((bool?)null);
        
        // act
        var result = await sut.EnterLocationManually(locationsService.Object, vacancyLocationService, model) as RedirectToRouteResult;
        
        // assert
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.AddMoreThanOneLocation_Get);
    }
    
    [Test, MoqAutoData]
    public async Task When_Posting_Valid_Data_Redirects_To_AddManyLocations_Route(
        Mock<IVacancyLocationService> vacancyLocationService,
        [Frozen] Mock<ILocationsService> locationsService,
        [Frozen] Vacancy vacancy,
        [Greedy] EnterLocationManuallyController sut)
    {
        // arrange
        int ukprn = new Random().Next();
        sut.AddControllerContext()
            .WithTempData()
            .WithUser(Guid.NewGuid())
            .WithClaim(ProviderRecruitClaims.IdamsUserUkprnClaimsTypeIdentifier, ukprn.ToString());
        sut.TempData.Add(TempDataKeys.AddLocationReturnPath, ReturnRoute);
        var model = new EnterLocationManuallyEditModel
        {
            VacancyId = vacancy.Id,
            Ukprn = ukprn,
            Origin = MultipleLocationsJourneyOrigin.Many,
            Wizard = true,
            AddressLine1 = "AddressLine1",
            AddressLine2 = "AddressLine2",
            City = "City",
            Postcode = Postcode
        };
        locationsService.Setup(x => x.IsPostcodeInEnglandAsync(Postcode)).ReturnsAsync(true);
        
        // act
        var result = await sut.EnterLocationManually(locationsService.Object, vacancyLocationService.Object, model) as RedirectToRouteResult;
        
        // assert
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.AddMoreThanOneLocation_Get);
        vacancyLocationService.Verify(x => x.SaveEmployerAddress(It.IsAny<VacancyUser>(), vacancy, ukprn, It.IsAny<Address>()), Times.Once);
    }
}