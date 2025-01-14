using System.Collections.Generic;
using System.Text.Json;
using AutoFixture.NUnit3;
using Esfa.Recruit.Provider.Web;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Controllers.Part1;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.Services;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.MultipleLocations;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NUnit.Framework;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Controllers.Part1;

public class MultipleLocationsControllerTests
{
    [Test, MoqAutoData]
    public async Task When_Getting_LocationsAvailability_Then_ViewModel_Is_Returned(
        Vacancy vacancy,
        Mock<IUtility> utility,
        [Greedy] MultipleLocationsController sut)
    {
        // arrange
        var vacancyRouteModel = new VacancyRouteModel
        {
            VacancyId = vacancy.Id,
            Ukprn = new Random().Next(),
        };
        utility.Setup(x => x.GetAuthorisedVacancyForEditAsync(vacancyRouteModel, It.IsAny<string>())).ReturnsAsync(vacancy);

        // act
        var result = (await sut.LocationAvailability(utility.Object, vacancyRouteModel) as ViewResult)?.Model as LocationAvailabilityViewModel;
        
        // assert
        result.Should().NotBeNull();
        result!.VacancyId.Should().Be(vacancy.Id);
        result.ApprenticeshipTitle.Should().Be(vacancy.Title);
        result.Ukprn.Should().Be(vacancyRouteModel.Ukprn);
        result.SelectedAvailability.Should().Be(AvailableWhere.OneLocation);
    }
    
    [Test, MoqAutoData]
    public async Task When_Posting_LocationsAvailability_As_The_Wizard_Then_You_Are_Redirected_To_The_AddMoreThanOneLocation_Route(
        [Greedy] MultipleLocationsController sut)
    {
        // arrange
        var editModel = new LocationAvailabilityEditModel
        {
            SelectedAvailability = AvailableWhere.MultipleLocations
        };
        
        // act
        var result = await sut.LocationAvailability(Mock.Of<IUtility>(), editModel, true) as RedirectToRouteResult;
        
        // assert
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.AddMoreThanOneLocation_Get);
    }
    
    [Test, MoqAutoData]
    public async Task When_Posting_LocationsAvailability_With_Invalid_state_Then_You_Are_Redirected_To_LocationAvailability(
        Vacancy vacancy,
        Mock<IUtility> utility,
        [Greedy] MultipleLocationsController sut)
    {
        // arrange
        var editModel = new LocationAvailabilityEditModel
        {
            Ukprn = new Random().Next(),
            SelectedAvailability = AvailableWhere.MultipleLocations,
            VacancyId = vacancy.Id,
        };
        utility.Setup(x => x.GetAuthorisedVacancyForEditAsync(editModel, It.IsAny<string>())).ReturnsAsync(vacancy);
        sut.ModelState.AddModelError(string.Empty, string.Empty);

        // act
        var result = (await sut.LocationAvailability(utility.Object, editModel, true) as ViewResult)?.Model as LocationAvailabilityViewModel;
        
        // assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(editModel);
    }
    
    [Test, MoqAutoData]
    public async Task When_Getting_AddMoreThanOneLocation_Then_The_View_Is_Returned(
        Vacancy vacancy,
        [Frozen] List<Address> locations,
        Mock<IVacancyLocationService> vacancyLocationService,
        Mock<IUtility> utility,
        VacancyRouteModel model,
        [Greedy] MultipleLocationsController sut)
    {
        // arrange
        var httpContext = new DefaultHttpContext();
        var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
        sut.TempData = tempData;
        sut.AddControllerContext().WithUser(Guid.NewGuid());

        vacancy.Id = model.VacancyId!.Value;
        utility.Setup(x => x.GetAuthorisedVacancyForEditAsync(model, It.IsAny<string>())).ReturnsAsync(vacancy);
        vacancyLocationService.Setup(x => x.GetVacancyLocations(vacancy, model.Ukprn)).ReturnsAsync(locations);
        
        // act
        var result = (await sut.AddMoreThanOneLocation(vacancyLocationService.Object, utility.Object, model, true) as ViewResult)?.Model as AddMoreThanOneLocationViewModel;

        // assert
        result.Should().NotBeNull();
        result!.ApprenticeshipTitle.Should().Be(vacancy.Title);
        result.AvailableLocations.Should().BeEquivalentTo(locations);
        result.VacancyId.Should().Be(vacancy.Id);
        result.Ukprn.Should().Be(model.Ukprn);
        result.SelectedLocations.Should().BeEquivalentTo(vacancy.EmployerLocations, options => options.ExcludingMissingMembers());
    }
    
    [Test, MoqAutoData]
    public async Task When_Getting_AddMoreThanOneLocation_Having_Returned_From_The_Add_New_Location_Pathway_Then_The_Previously_Selected_Values_Are_Restored(
        Vacancy vacancy,
        [Frozen] List<Address> locations,
        Mock<IVacancyLocationService> vacancyLocationService,
        Mock<IUtility> utility,
        VacancyRouteModel model,
        [Greedy] MultipleLocationsController sut)
    {
        // arrange
        var selected = new List<string> { "1", "2", "3" };
        var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>())
        {
            [TempDataKeys.SelectedLocations] = JsonSerializer.Serialize(selected)
        };
        sut.TempData = tempData;
    
        sut.AddControllerContext().WithUser(Guid.NewGuid());

        vacancy.Id = model.VacancyId!.Value;
        utility.Setup(x => x.GetAuthorisedVacancyForEditAsync(model, It.IsAny<string>())).ReturnsAsync(vacancy);
        vacancyLocationService.Setup(x => x.GetVacancyLocations(vacancy, model.Ukprn)).ReturnsAsync(locations);
        
        // act
        var result = (await sut.AddMoreThanOneLocation(vacancyLocationService.Object, utility.Object, model, true) as ViewResult)?.Model as AddMoreThanOneLocationViewModel;
    
        // assert
        result.Should().NotBeNull();
        result!.SelectedLocations.Should().BeEquivalentTo(selected);
    }

    [Test, MoqAutoData]
    public async Task When_Posting_AddMoreThanOneLocation_With_Invalid_state_Then_The_View_Is_Returned(
        Vacancy vacancy,
        [Frozen] List<Address> locations,
        Mock<IVacancyLocationService> vacancyLocationService,
        Mock<IUtility> utility,
        AddMoreThanOneLocationEditModel model,
        [Greedy] MultipleLocationsController sut)
    {
        // arrange
        sut
            .AddControllerContext()
            .WithUser(Guid.NewGuid())
            .WithClaim(ProviderRecruitClaims.IdamsUserUkprnClaimsTypeIdentifier, model.Ukprn.ToString());
        var httpContext = new DefaultHttpContext();
        var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
        sut.TempData = tempData;

        vacancy.Id = model.VacancyId!.Value;
        utility.Setup(x => x.GetAuthorisedVacancyForEditAsync(model, It.IsAny<string>())).ReturnsAsync(vacancy);
        vacancyLocationService.Setup(x => x.GetVacancyLocations(vacancy, model.Ukprn)).ReturnsAsync(locations);

        // act
        var result = (await sut.AddMoreThanOneLocation(vacancyLocationService.Object, utility.Object, model, true) as ViewResult)?.Model as AddMoreThanOneLocationViewModel;

        // assert
        result.Should().NotBeNull();
        result!.ApprenticeshipTitle.Should().Be(vacancy.Title);
        result.AvailableLocations.Should().BeEquivalentTo(locations);
        result.Ukprn.Should().Be(model.Ukprn);
        result.VacancyId.Should().Be(vacancy.Id);
        result.SelectedLocations.Should().BeEquivalentTo(model.SelectedLocations);
    }
    
    [Test, MoqAutoData]
    public async Task When_Posting_AddMoreThanOneLocation_With_Valid_state_Then_You_Are_Redirected_To_Confirmation_Route(
        Vacancy vacancy,
        [Frozen] List<Address> locations,
        Mock<IVacancyLocationService> vacancyLocationService,
        Mock<IUtility> utility,
        AddMoreThanOneLocationEditModel model,
        [Greedy] MultipleLocationsController sut)
    {
        // arrange
        var userId = Guid.NewGuid();
        sut
            .AddControllerContext()
            .WithUser(userId)
            .WithClaim(ProviderRecruitClaims.IdamsUserUkprnClaimsTypeIdentifier, model.Ukprn.ToString());
        vacancy.Id = model.VacancyId!.Value;
        utility.Setup(x => x.GetAuthorisedVacancyForEditAsync(model, It.IsAny<string>())).ReturnsAsync(vacancy);
        vacancyLocationService.Setup(x => x.GetVacancyLocations(vacancy, model.Ukprn)).ReturnsAsync(locations);
        vacancyLocationService
            .Setup(x => x.UpdateDraftVacancyLocations(vacancy, It.Is<VacancyUser>(u => u.UserId == userId.ToString()),
                AvailableWhere.MultipleLocations, It.IsAny<List<Address>>(), null))
            .ReturnsAsync(new UpdateVacancyLocationsResult(null));
        
        // act
        var result = await sut.AddMoreThanOneLocation(vacancyLocationService.Object, utility.Object, model, true) as RedirectToRouteResult;
        
        // assert
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.MultipleLocationsConfirm_Get);
    }
    
    [Test, MoqAutoData]
    public void When_Getting_AddALocation_The_Selected_Values_Are_Stored(
        [Greedy] MultipleLocationsController sut)
    {
        // arrange
        var selected = new List<string> { "1", "2", "3" };
        string jsonSelected = JsonSerializer.Serialize(selected);
        var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
        sut.TempData = tempData;
        var editModel = new AddMoreThanOneLocationEditModel { SelectedLocations = selected };
        
        // act
        sut.AddALocation(editModel, true);

        // assert
        tempData.Keys.Should().Contain(TempDataKeys.SelectedLocations);
        tempData.Values.Should().Contain(jsonSelected);
    }
    
    [Test, MoqAutoData]
    public void When_Getting_AddALocation_Result_Is_Redirection_To_The_Correct_Route(
        [Greedy] MultipleLocationsController sut)
    {
        // arrange
        var selected = new List<string> { "1", "2", "3" };
        var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
        sut.TempData = tempData;
        var editModel = new AddMoreThanOneLocationEditModel { SelectedLocations = selected };
        
        // act
        var result = sut.AddALocation(editModel, true) as RedirectToRouteResult;

        // assert
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.AddLocation_Get);
    }
    
    [Test, MoqAutoData]
    public async Task When_Getting_ConfirmLocations_Then_The_View_Is_Returned(
        Vacancy vacancy,
        Mock<IUtility> utility,
        [Greedy] MultipleLocationsController sut)
    {
        // arrange
        var model = new VacancyRouteModel
        {
            VacancyId = vacancy.Id,
            Ukprn = new Random().Next(),
        };
        utility.Setup(x => x.GetAuthorisedVacancyForEditAsync(model, It.IsAny<string>())).ReturnsAsync(vacancy);

        // act
        var result = (await sut.ConfirmLocations(utility.Object, model, true) as ViewResult)?.Model as ConfirmLocationsViewModel;

        // assert
        result.Should().NotBeNull();
        result!.VacancyId.Should().Be(vacancy.Id);
        result.ApprenticeshipTitle.Should().Be(vacancy.Title);
        result.Ukprn.Should().Be(model.Ukprn);
        result.Locations.Should().BeEquivalentTo(vacancy.EmployerLocations);
    }
    
    [Test, MoqAutoData]
    public void When_Posting_ConfirmLocations_Then_A_Redirect_Result_Is_Returned(
        bool wizard,
        string expectedRedirectRoute,
        Vacancy vacancy,
        VacancyRouteModel model,
        [Greedy] MultipleLocationsController sut)
    {
        // act
        var result = sut.ConfirmLocations(model, wizard) as RedirectToRouteResult;

        // assert
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.ProviderTaskListGet);
    }
}