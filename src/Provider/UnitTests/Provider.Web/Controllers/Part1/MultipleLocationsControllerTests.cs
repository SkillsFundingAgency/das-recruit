using AutoFixture.NUnit3;
using Esfa.Recruit.Provider.Web;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Controllers.Part1;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.MultipleLocations;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
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
        utility.Setup(x => x.GetAuthorisedVacancyForEditAsync(It.IsAny<VacancyRouteModel>(), It.IsAny<string>())).ReturnsAsync(vacancy);
        
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
        utility.Setup(x => x.GetAuthorisedVacancyForEditAsync(It.IsAny<VacancyRouteModel>(), It.IsAny<string>())).ReturnsAsync(vacancy);
        sut.ModelState.AddModelError(string.Empty, string.Empty);
        
        // act
        var result = (await sut.LocationAvailability(utility.Object, editModel, true) as ViewResult)?.Model as LocationAvailabilityViewModel;
        
        // assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(editModel);
    }
}