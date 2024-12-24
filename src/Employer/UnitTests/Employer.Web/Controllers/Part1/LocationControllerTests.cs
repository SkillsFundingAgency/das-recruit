using System.Collections.Generic;
using System.Linq;
using AutoFixture.NUnit3;
using Esfa.Recruit.Employer.Web;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Controllers.Part1;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.Services;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Location;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Controllers.Part1;

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
        model.EmployerAccountId = vacancy.EmployerAccountId;
        utility.Setup(x => x.GetAuthorisedVacancyForEditAsync(It.IsAny<VacancyRouteModel>(), It.IsAny<string>())).ReturnsAsync(vacancy);
        _vacancyLocationService.Setup(x => x.GetVacancyLocations(It.Is<Vacancy>(v => v == vacancy))).ReturnsAsync(locations);
        
        // act
        var result = (await _sut.AddOneLocation(_vacancyLocationService.Object, utility.Object, model, true) as ViewResult)?.Model as AddOneLocationViewModel;

        // assert
        result.Should().NotBeNull();
        result!.VacancyId.Should().Be(vacancy.Id);
        result.ApprenticeshipTitle.Should().Be(vacancy.Title);
        result.EmployerAccountId.Should().BeEquivalentTo(vacancy.EmployerAccountId);
        result.AvailableLocations.Should().BeEquivalentTo(locations);
    }
    
    [Test, MoqAutoData]
    public async Task When_Getting_AddOneLocation_View_Is_Returned_With_Added_Address([Frozen] Vacancy vacancy, [Frozen] Mock<IUtility> utility, VacancyRouteModel model, [Frozen] List<Address> locations)
    {
        // arrange
        model.VacancyId = vacancy.Id;
        model.EmployerAccountId = vacancy.EmployerAccountId;
        utility.Setup(x => x.GetAuthorisedVacancyForEditAsync(It.IsAny<VacancyRouteModel>(), It.IsAny<string>())).ReturnsAsync(vacancy);
        _vacancyLocationService.Setup(x => x.GetVacancyLocations(It.Is<Vacancy>(v => v == vacancy))).ReturnsAsync(locations);
        
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
        model.EmployerAccountId = vacancy.EmployerAccountId;
        utility.Setup(x => x.GetAuthorisedVacancyForEditAsync(It.IsAny<VacancyRouteModel>(), It.IsAny<string>())).ReturnsAsync(vacancy);
        _vacancyLocationService.Setup(x => x.GetVacancyLocations(It.Is<Vacancy>(v => v == vacancy))).ReturnsAsync(locations);
        
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
        model.EmployerAccountId = vacancy.EmployerAccountId;
        utility.Setup(x => x.GetAuthorisedVacancyForEditAsync(It.IsAny<VacancyRouteModel>(), It.IsAny<string>())).ReturnsAsync(vacancy);
        _vacancyLocationService.Setup(x => x.GetVacancyLocations(It.Is<Vacancy>(v => v == vacancy))).ReturnsAsync(locations);
        
        // act
        var result = (await _sut.AddOneLocation(_vacancyLocationService.Object, utility.Object, model, true) as ViewResult)?.Model as AddOneLocationViewModel;

        // assert
        result.Should().NotBeNull();
        result!.SelectedLocation.Should().Be(address.ToAddressString());
    }

    [Test]
    [MoqInlineAutoData(true, RouteNames.EmployerTaskListGet)]
    [MoqInlineAutoData(false, RouteNames.EmployerCheckYourAnswersGet)]
    public async Task When_Posting_AddOneLocation_With_Valid_Model_Then_Redirected(bool wizard, string expectedRouteName,
        [Frozen] Vacancy vacancy, [Frozen] Mock<IUtility> utility, [Frozen] List<Address> locations)
    {
        // arrange
        var model = new AddOneLocationEditModel
        {
            VacancyId = vacancy.Id,
            EmployerAccountId = vacancy.EmployerAccountId,
            SelectedLocation = locations.First().ToAddressString()
        };
        utility.Setup(x => x.GetAuthorisedVacancyForEditAsync(It.IsAny<VacancyRouteModel>(), It.IsAny<string>())).ReturnsAsync(vacancy);
        _vacancyLocationService.Setup(x => x.GetVacancyLocations(It.Is<Vacancy>(v => v == vacancy))).ReturnsAsync(locations);
        _vacancyLocationService.Setup(x => x.UpdateDraftVacancyLocations(It.IsAny<Vacancy>(), It.IsAny<VacancyUser>(), It.IsAny<AvailableWhere>(), It.IsAny<List<Address>>(), null))
            .ReturnsAsync(new UpdateVacancyLocationsResult(null));
        
        // act
        var result = await _sut.AddOneLocation(_vacancyLocationService.Object, utility.Object, model, wizard) as RedirectToRouteResult;
    
        // assert
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(expectedRouteName);
    }
    
    [Test, MoqAutoData]
    public async Task When_Posting_AddOneLocation_With_Invalid_Model_Then_View_Returned([Frozen] Vacancy vacancy, [Frozen] Mock<IUtility> utility, [Frozen] List<Address> locations)
    {
        // arrange
        var model = new AddOneLocationEditModel
        {
            VacancyId = vacancy.Id,
            EmployerAccountId = vacancy.EmployerAccountId,
            SelectedLocation = "invalid"
        };
        utility.Setup(x => x.GetAuthorisedVacancyForEditAsync(It.IsAny<VacancyRouteModel>(), It.IsAny<string>())).ReturnsAsync(vacancy);
        _vacancyLocationService.Setup(x => x.GetVacancyLocations(It.Is<Vacancy>(v => v == vacancy))).ReturnsAsync(locations);
        _vacancyLocationService.Setup(x => x.UpdateDraftVacancyLocations(It.IsAny<Vacancy>(), It.IsAny<VacancyUser>(), It.IsAny<AvailableWhere>(), It.IsAny<List<Address>>(), null))
            .ReturnsAsync(new UpdateVacancyLocationsResult(new EntityValidationResult { Errors = [new EntityValidationError(1, "EmployerLocations", "error message", "code")] }));
        
        // act
        var result = (await _sut.AddOneLocation(_vacancyLocationService.Object, utility.Object, model, true) as ViewResult)?.Model as AddOneLocationViewModel;
        var errorResult = _sut.ModelState.GetValueOrDefault("SelectedLocation");
    
        // assert
        result.Should().NotBeNull();
        result!.VacancyId.Should().Be(vacancy.Id);
        result.ApprenticeshipTitle.Should().Be(vacancy.Title);
        result.EmployerAccountId.Should().BeEquivalentTo(vacancy.EmployerAccountId);
        result.AvailableLocations.Should().BeEquivalentTo(locations);
        result.SelectedLocation.Should().Be("invalid");
        _sut.ModelState.Values.Should().HaveCount(1);
        errorResult.Should().NotBeNull();
        errorResult.Errors.Should().HaveCount(1);
        errorResult.Errors.First().ErrorMessage.Should().Be("error message");

    }
}