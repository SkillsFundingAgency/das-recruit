using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using AutoFixture.NUnit3;
using Esfa.Recruit.Employer.Web;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Controllers.Part1;
using Esfa.Recruit.Employer.Web.Orchestrators.Part1;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.MultipleLocations;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Controllers.Part1;

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
            EmployerAccountId = vacancy.EmployerAccountId,
        };
        utility.Setup(x => x.GetAuthorisedVacancyForEditAsync(It.IsAny<VacancyRouteModel>(), It.IsAny<string>())).ReturnsAsync(vacancy);
        
        // act
        var result = (await sut.LocationAvailability(utility.Object, vacancyRouteModel) as ViewResult)?.Model as LocationAvailabilityViewModel;
        
        // assert
        result.Should().NotBeNull();
        result!.VacancyId.Should().Be(vacancy.Id);
        result.ApprenticeshipTitle.Should().Be(vacancy.Title);
        result.EmployerAccountId.Should().BeEquivalentTo(vacancy.EmployerAccountId);
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
            EmployerAccountId = vacancy.EmployerAccountId,
            SelectedAvailability = AvailableWhere.MultipleLocations,
            VacancyId = vacancy.Id,
        };
        utility.Setup(x => x.GetAuthorisedVacancyForEditAsync(It.IsAny<VacancyRouteModel>(), It.IsAny<string>())).ReturnsAsync(vacancy);
        sut.ModelState.AddModelError(string.Empty, string.Empty);
        
        // act
        var result = (await sut.LocationAvailability(utility.Object, editModel, true) as ViewResult)?.Model as LocationAvailabilityViewModel;
        
        // assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(editModel);
    }

    [Test, MoqAutoData]
    public async Task When_Getting_AddMoreThanOneLocation_Then_The_View_Is_Returned(
        Mock<IMultipleLocationsOrchestrator> orchestrator,
        AddMoreThanOneLocationViewModel viewModel,
        [Greedy] MultipleLocationsController sut)
    {
        // arrange
        var httpContext = new DefaultHttpContext();
        var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
        sut.TempData = tempData;

        orchestrator
            .Setup(x => x.GetAddMoreThanOneLocationViewModelAsync(It.IsAny<VacancyRouteModel>(), It.Is<bool>(wizard => wizard == true)))
            .ReturnsAsync(viewModel);
        
        // act
        var result = (await sut.AddMoreThanOneLocation(orchestrator.Object, new VacancyRouteModel(), true) as ViewResult)?.Model as AddMoreThanOneLocationViewModel;

        // assert
        result.Should().NotBeNull();
        result.Should().Be(viewModel);
    }
    
    [Test, MoqAutoData]
    public async Task When_Getting_AddMoreThanOneLocation_Having_Returned_From_The_Add_New_Location_Pathway_Then_The_Previously_Selected_Values_Are_Restored(
        Mock<IMultipleLocationsOrchestrator> orchestrator,
        AddMoreThanOneLocationViewModel viewModel,
        [Greedy] MultipleLocationsController sut)
    {
        // arrange
        var selected = new List<string> { "1", "2", "3" };
        var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>())
        {
            [TempDataKeys.SelectedLocations] = JsonSerializer.Serialize(selected)
        };
        sut.TempData = tempData;

        orchestrator
            .Setup(x => x.GetAddMoreThanOneLocationViewModelAsync(It.IsAny<VacancyRouteModel>(), It.Is<bool>(wizard => wizard == true)))
            .ReturnsAsync(viewModel);
        
        // act
        var result = (await sut.AddMoreThanOneLocation(orchestrator.Object, new VacancyRouteModel(), true) as ViewResult)?.Model as AddMoreThanOneLocationViewModel;

        // assert
        result.Should().NotBeNull();
        result!.SelectedLocations.Should().BeEquivalentTo(selected);
    }
    
    [Test, MoqAutoData]
    public async Task When_Posting_AddMoreThanOneLocation_With_Invalid_state_Then_The_View_Is_Returned(
        Mock<IMultipleLocationsOrchestrator> orchestrator,
        AddMoreThanOneLocationEditModel editModel,
        [Greedy] MultipleLocationsController sut)
    {
        // arrange
        var error = new EntityValidationError(1, "property-name", "error-message", "error-code");
        var orchestratorResponse = new OrchestratorResponse(new EntityValidationResult { Errors = [error] });
        sut.AddControllerContext().WithUser(Guid.NewGuid());
        orchestrator
            .Setup(x => x.PostAddMoreThanOneLocationViewModelAsync(It.IsAny<AddMoreThanOneLocationEditModel>(), It.IsAny<VacancyUser>()))
            .ReturnsAsync(orchestratorResponse);
        
        // act
        var result = await sut.AddMoreThanOneLocation(orchestrator.Object, editModel, true) as ViewResult;
        
        // assert
        result.Should().NotBeNull();
        result!.Model.Should().NotBeNull();
        result.Model.Should().BeOfType<AddMoreThanOneLocationViewModel>();
        sut.ControllerContext.ModelState.Count.Should().Be(1);
        sut.ControllerContext.ModelState.First().Key.Should().Be(error.PropertyName);
        sut.ControllerContext.ModelState.First().Value.Errors.First().ErrorMessage.Should().Be(error.ErrorMessage);
    }
    
    [Test, MoqAutoData]
    public async Task When_Posting_AddMoreThanOneLocation_With_Valid_state_Then_You_Are_Redirected_To_Confirmation_Route(
        Mock<IMultipleLocationsOrchestrator> orchestrator,
        AddMoreThanOneLocationEditModel editModel,
        [Greedy] MultipleLocationsController sut)
    {
        // arrange
        sut.AddControllerContext().WithUser(Guid.NewGuid());
        orchestrator
            .Setup(x => x.PostAddMoreThanOneLocationViewModelAsync(It.IsAny<AddMoreThanOneLocationEditModel>(), It.IsAny<VacancyUser>()))
            .ReturnsAsync(new OrchestratorResponse(false));
        
        // act
        var result = (await sut.AddMoreThanOneLocation(orchestrator.Object, editModel, true) as ViewResult)?.Model as AddMoreThanOneLocationViewModel;
        
        // assert
        result.Should().NotBeNull();
        result!.SelectedLocations.Should().BeEquivalentTo(editModel.SelectedLocations);
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
        var vacancyRouteModel = new VacancyRouteModel
        {
            VacancyId = vacancy.Id,
            EmployerAccountId = vacancy.EmployerAccountId,
        };
        utility.Setup(x => x.GetAuthorisedVacancyForEditAsync(It.IsAny<VacancyRouteModel>(), It.IsAny<string>())).ReturnsAsync(vacancy);
        
        // act
        var result = (await sut.ConfirmLocations(utility.Object, vacancyRouteModel, true) as ViewResult)?.Model as ConfirmLocationsViewModel;

        // assert
        result.Should().NotBeNull();
        result!.VacancyId.Should().Be(vacancy.Id);
        result.ApprenticeshipTitle.Should().Be(vacancy.Title);
        result.EmployerAccountId.Should().BeEquivalentTo(vacancy.EmployerAccountId);
        result.Locations.Should().BeEquivalentTo(vacancy.EmployerLocations);
    }
    
    [Test]
    [InlineAutoData(true, RouteNames.EmployerTaskListGet)]
    [InlineAutoData(false, RouteNames.EmployerCheckYourAnswersGet)]
    public void When_Posting_ConfirmLocations_Then_A_Redirect_Result_Is_Returned(
        bool wizard,
        string expectedRedirectRoute,
        Vacancy vacancy,
        [Greedy] MultipleLocationsController sut)
    {
        // arrange
        var vacancyRouteModel = new VacancyRouteModel
        {
            VacancyId = vacancy.Id,
            EmployerAccountId = vacancy.EmployerAccountId,
        };
        
        // act
        var result = sut.ConfirmLocations(vacancyRouteModel, wizard) as RedirectToRouteResult;

        // assert
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(expectedRedirectRoute);
    }
}