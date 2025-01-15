using System.Collections.Generic;
using System.Linq;
using AutoFixture.NUnit3;
using Esfa.Recruit.Provider.Web;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Controllers.Part1;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.Services;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.Location;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NUnit.Framework;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Controllers.Part1;

public class LocationControllerTests
{
    private LocationController _sut;
    private Mock<IVacancyLocationService> _vacancyLocationService;

    [SetUp]
    public void SetUp()
    {
        _sut = new LocationController(Mock.Of<IWebHostEnvironment>());
        _sut.AddControllerContext().WithUser(Guid.NewGuid());
        var tempData = new TempDataDictionary(_sut.ControllerContext.HttpContext, Mock.Of<ITempDataProvider>());
        _sut.TempData = tempData;

        _vacancyLocationService = new Mock<IVacancyLocationService>();
    }

    [Test, MoqAutoData]
    public async Task When_Getting_AddOneLocation_View_Is_Returned([Frozen] Vacancy vacancy, [Frozen] Mock<IUtility> utility, VacancyRouteModel model, [Frozen] List<Address> locations)
    {
        // arrange
        model.VacancyId = vacancy.Id;
        utility.Setup(x => x.GetAuthorisedVacancyForEditAsync(model, RouteNames.AddOneLocation_Get)).ReturnsAsync(vacancy);
        _vacancyLocationService.Setup(x => x.GetVacancyLocations(vacancy, model.Ukprn)).ReturnsAsync(locations);
        
        // act
        var result = (await _sut.AddOneLocation(_vacancyLocationService.Object, utility.Object, model, true) as ViewResult)?.Model as AddOneLocationViewModel;

        // assert
        result.Should().NotBeNull();
        result!.VacancyId.Should().Be(vacancy.Id);
        result.ApprenticeshipTitle.Should().Be(vacancy.Title);
        result.Ukprn.Should().Be(model.Ukprn);
        result.AvailableLocations.Should().BeEquivalentTo(locations);
    }
    
    [Test, MoqAutoData]
    public async Task When_Getting_AddOneLocation_View_Is_Returned_With_Added_Address([Frozen] Vacancy vacancy, [Frozen] Mock<IUtility> utility, VacancyRouteModel model, [Frozen] List<Address> locations)
    {
        // arrange
        model.VacancyId = vacancy.Id;
        utility.Setup(x => x.GetAuthorisedVacancyForEditAsync(model, RouteNames.AddOneLocation_Get)).ReturnsAsync(vacancy);
        _vacancyLocationService.Setup(x => x.GetVacancyLocations(vacancy, model.Ukprn)).ReturnsAsync(locations);
        
        const string newlyAddedAddress = "An Address";
        _sut.TempData[TempDataKeys.AddedLocation] = newlyAddedAddress;
        
        // act
        var result = (await _sut.AddOneLocation(_vacancyLocationService.Object, utility.Object, model, true) as ViewResult)?.Model as AddOneLocationViewModel;

        // assert
        result.Should().NotBeNull();
        result!.BannerAddress.Should().Be(newlyAddedAddress);
        result.SelectedLocation.Should().Be(newlyAddedAddress);
    }
    
    [Test, MoqAutoData]
    public async Task When_Getting_AddOneLocation_SelectedLocation_Is_Null_When_Vacancy_Has_More_Than_One_Existing_Address([Frozen] Vacancy vacancy, [Frozen] Mock<IUtility> utility, VacancyRouteModel model, [Frozen] List<Address> locations)
    {
        // arrange
        model.VacancyId = vacancy.Id;
        utility.Setup(x => x.GetAuthorisedVacancyForEditAsync(model, RouteNames.AddOneLocation_Get)).ReturnsAsync(vacancy);
        _vacancyLocationService.Setup(x => x.GetVacancyLocations(vacancy, model.Ukprn)).ReturnsAsync(locations);
        
        // act
        var result = (await _sut.AddOneLocation(_vacancyLocationService.Object, utility.Object, model, true) as ViewResult)?.Model as AddOneLocationViewModel;

        // assert
        result.Should().NotBeNull();
        result!.SelectedLocation.Should().BeNull();
    }
    
    [Test, MoqAutoData]
    public async Task When_Getting_AddOneLocation_SelectedLocation_Is_Set_When_Vacancy_Has_Exactly_One_Existing_Address(
        [Frozen] Vacancy vacancy,
        [Frozen] Mock<IUtility> utility,
        VacancyRouteModel model,
        Address address,
        [Frozen] List<Address> locations)
    {
        // arrange
        vacancy.EmployerLocations = [address];
        model.VacancyId = vacancy.Id;
        utility.Setup(x => x.GetAuthorisedVacancyForEditAsync(model, RouteNames.AddOneLocation_Get)).ReturnsAsync(vacancy);
        _vacancyLocationService.Setup(x => x.GetVacancyLocations(vacancy, model.Ukprn)).ReturnsAsync(locations);
        
        // act
        var result = (await _sut.AddOneLocation(_vacancyLocationService.Object, utility.Object, model, true) as ViewResult)?.Model as AddOneLocationViewModel;

        // assert
        result.Should().NotBeNull();
        result!.SelectedLocation.Should().Be(address.ToAddressString());
    }

    [Test, MoqAutoData]
    public async Task When_Posting_AddOneLocation_With_Valid_Model_Then_Redirected(
        [Frozen] Vacancy vacancy,
        [Frozen] Mock<IUtility> utility,
        [Frozen] List<Address> locations)
    {
        // arrange
        var model = new AddOneLocationEditModel
        {
            VacancyId = vacancy.Id,
            Ukprn = new Random().Next(),
            SelectedLocation = locations.First().ToAddressString()
        };
        utility.Setup(x => x.GetAuthorisedVacancyForEditAsync(model, RouteNames.AddOneLocation_Post)).ReturnsAsync(vacancy);
        _vacancyLocationService.Setup(x => x.GetVacancyLocations(vacancy, model.Ukprn)).ReturnsAsync(locations);
        _vacancyLocationService
            .Setup(x => x.UpdateDraftVacancyLocations(vacancy, It.IsAny<VacancyUser>(), AvailableWhere.OneLocation, It.IsAny<List<Address>>(), null))
            .ReturnsAsync(new UpdateVacancyLocationsResult(null));
        
        _sut.AddControllerContext()
            .WithTempData()
            .WithUser(Guid.NewGuid())
            .WithClaim(ProviderRecruitClaims.IdamsUserUkprnClaimsTypeIdentifier, model.Ukprn.ToString());
        
        // act
        var result = await _sut.AddOneLocation(_vacancyLocationService.Object, utility.Object, model, true) as RedirectToRouteResult;
    
        // assert
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.ProviderTaskListGet);
    }
    
    [Test, MoqAutoData]
    public async Task When_Posting_AddOneLocation_With_Invalid_Model_Then_View_Returned([Frozen] Vacancy vacancy, [Frozen] Mock<IUtility> utility, [Frozen] List<Address> locations)
    {
        // arrange
        var model = new AddOneLocationEditModel
        {
            VacancyId = vacancy.Id,
            Ukprn = new Random().Next(),
            SelectedLocation = "invalid"
        };
        utility.Setup(x => x.GetAuthorisedVacancyForEditAsync(model, RouteNames.AddOneLocation_Post)).ReturnsAsync(vacancy);
        _vacancyLocationService.Setup(x => x.GetVacancyLocations(vacancy, model.Ukprn)).ReturnsAsync(locations);
        _vacancyLocationService
            .Setup(x => x.UpdateDraftVacancyLocations(vacancy, It.IsAny<VacancyUser>(), AvailableWhere.OneLocation, It.IsAny<List<Address>>(), null))
            .ReturnsAsync(new UpdateVacancyLocationsResult(new EntityValidationResult { Errors = [new EntityValidationError(1, "EmployerLocations", "error message", "code")] }));
        _sut.AddControllerContext()
            .WithTempData()
            .WithUser(Guid.NewGuid())
            .WithClaim(ProviderRecruitClaims.IdamsUserUkprnClaimsTypeIdentifier, model.Ukprn.ToString());
        
        // act
        var result = (await _sut.AddOneLocation(_vacancyLocationService.Object, utility.Object, model, true) as ViewResult)?.Model as AddOneLocationViewModel;
        var errorResult = _sut.ModelState.GetValueOrDefault("SelectedLocation");
    
        // assert
        result.Should().NotBeNull();
        result!.VacancyId.Should().Be(vacancy.Id);
        result.ApprenticeshipTitle.Should().Be(vacancy.Title);
        result.Ukprn.Should().Be(model.Ukprn);
        result.AvailableLocations.Should().BeEquivalentTo(locations);
        result.SelectedLocation.Should().Be("invalid");
        _sut.ModelState.Values.Should().HaveCount(1);
        errorResult.Should().NotBeNull();
        errorResult.Errors.Should().HaveCount(1);
        errorResult.Errors.First().ErrorMessage.Should().Be("error message");
    }
}