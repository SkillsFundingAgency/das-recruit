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
    [Test]
    [MoqInlineAutoData(true, RouteNames.ProviderCheckYourAnswersGet)]
    [MoqInlineAutoData(false, RouteNames.NumberOfPositions_Get)]
    public async Task When_Getting_LocationsAvailability_Then_ViewModel_Is_Returned(
        bool isTaskListCompleted,
        string expectedPageBackLink,
        [Frozen] Vacancy vacancy,
        [Frozen] Mock<IUtility> utility,
        [Greedy] MultipleLocationsController sut)
    {
        // arrange
        var vacancyRouteModel = new VacancyRouteModel
        {
            VacancyId = vacancy.Id,
            Ukprn = new Random().Next(),
        };

        utility.Setup(x => x.IsTaskListCompleted(vacancy)).Returns(isTaskListCompleted);

        // act
        var result = (await sut.LocationAvailability(vacancyRouteModel) as ViewResult)?.Model as LocationAvailabilityViewModel;
        
        // assert
        result.Should().NotBeNull();
        result!.VacancyId.Should().Be(vacancy.Id);
        result.ApprenticeshipTitle.Should().Be(vacancy.Title);
        result.SelectedAvailability.Should().Be(AvailableWhere.OneLocation);
        result.IsTaskListCompleted.Should().Be(isTaskListCompleted);
        result.PageBackLink.Should().Be(expectedPageBackLink);
    }
    
    [Test, MoqAutoData]
    public async Task When_Posting_LocationsAvailability_Then_You_Are_Redirected_To_The_Task_List(
        LocationAvailabilityEditModel locationAvailabilityEditModel,
        [Greedy] MultipleLocationsController sut)
    {
        // act
        var result = await sut.LocationAvailability(locationAvailabilityEditModel, true) as RedirectToRouteResult;
        
        // assert
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.ProviderTaskListGet);
    }
}