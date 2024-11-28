using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using AutoFixture.NUnit3;
using Esfa.Recruit.Employer.Web;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Controllers.Part1;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.MultipleLocations;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Controllers.Part1;

public class MultipleLocationsControllerTests
{
    [Test, MoqAutoData]
    public async Task When_Getting_LocationsAvailability_Then_ViewModel_Is_Returned(
        [Frozen] Vacancy vacancy,
        [Frozen] Mock<IUtility> utility,
        [Greedy] MultipleLocationsController sut)
    {
        // arrange
        var vacancyRouteModel = new VacancyRouteModel
        {
            VacancyId = vacancy.Id,
            EmployerAccountId = vacancy.EmployerAccountId,
        };
        
        // act
        var result = (await sut.LocationAvailability(vacancyRouteModel) as ViewResult)?.Model as LocationAvailabilityViewModel;
        
        // assert
        result.Should().NotBeNull();
        result!.VacancyId.Should().Be(vacancy.Id);
        result.ApprenticeshipTitle.Should().Be(vacancy.Title);
        result.EmployerAccountId.Should().BeEquivalentTo(vacancy.EmployerAccountId);
        result.SelectedAvailability.Should().Be(AvailableWhere.OneLocation);
    }
    
    [Test, MoqAutoData]
    public async Task When_Posting_LocationsAvailability_As_The_Wizard_Then_You_Are_Redirected_To_The_Task_List(
        LocationAvailabilityEditModel locationAvailabilityEditModel,
        [Greedy] MultipleLocationsController sut)
    {
        // act
        var result = await sut.LocationAvailability(locationAvailabilityEditModel, true) as RedirectToRouteResult;
        
        // assert
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.EmployerTaskListGet);
    }
    
    [Test, MoqAutoData]
    public async Task When_Posting_LocationsAvailability_Not_As_The_Wizard_Then_You_Are_Redirected_To_Check_Your_Answers(
        LocationAvailabilityEditModel locationAvailabilityEditModel,
        [Greedy] MultipleLocationsController sut)
    {
        // act
        var result = await sut.LocationAvailability(locationAvailabilityEditModel, false) as RedirectToRouteResult;
        
        // assert
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.EmployerCheckYourAnswersGet);
    }
    
    [Test, MoqAutoData]
    public async Task When_Posting_LocationsAvailability_With_Invalid_state_Then_You_Are_Redirected_To_LocationAvailability(
        LocationAvailabilityEditModel locationAvailabilityEditModel,
        [Greedy] MultipleLocationsController sut)
    {
        // arrange
        sut.ModelState.AddModelError(string.Empty, string.Empty);
        
        // act
        var result = (await sut.LocationAvailability(locationAvailabilityEditModel) as ViewResult)?.Model as LocationAvailabilityViewModel;
        
        // assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(locationAvailabilityEditModel);
    }
}