using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Controllers.Part1;
using Esfa.Recruit.Employer.Web.Orchestrators.Part1;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.TrainingProvider;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Controllers.Part1.TrainingProviderControllerTests;

public class WhenGettingSelectTrainingProvider
{
    [Test, MoqAutoData]
    public async Task Then_Redirected_To_No_Providers_View_When_No_Training_Providers_Found_For_Course(
        VacancyRouteModel model,
        SelectTrainingProviderViewModel selectTrainingProviderViewModel,
        Mock<TrainingProviderOrchestrator> orchestrator)
    {
        // arrange
        selectTrainingProviderViewModel.TrainingProviders = [];
        orchestrator
            .Setup(x => x.GetSelectTrainingProviderViewModelAsync(model, null))
            .ReturnsAsync(selectTrainingProviderViewModel);

        var sut = new TrainingProviderController(orchestrator.Object);

        // act
        var result = await sut.SelectTrainingProvider(model, "true", string.Empty) as ViewResult;

        // assert
        result.Should().NotBeNull();
        result!.ViewName.Should().Be(ViewNames.NoAvailableProviders);
    }
    
    [Test, MoqAutoData]
    public async Task Then_View_Is_Returned_When_Training_Providers_Found_For_Course(
        VacancyRouteModel model,
        Mock<ITempDataDictionary> tempData,
        SelectTrainingProviderViewModel selectTrainingProviderViewModel,
        Mock<TrainingProviderOrchestrator> orchestrator)
    {
        // arrange
        orchestrator
            .Setup(x => x.GetSelectTrainingProviderViewModelAsync(model, null))
            .ReturnsAsync(selectTrainingProviderViewModel);

        var sut = new TrainingProviderController(orchestrator.Object);
        sut.TempData = tempData.Object;

        // act
        var result = await sut.SelectTrainingProvider(model, "true", string.Empty) as ViewResult;

        // assert
        result.Should().NotBeNull();
        result!.ViewName.Should().BeNull();
    }
}