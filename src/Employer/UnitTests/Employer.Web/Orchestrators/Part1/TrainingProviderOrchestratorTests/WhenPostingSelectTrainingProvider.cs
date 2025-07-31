using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Employer.Web;
using Esfa.Recruit.Employer.Web.Orchestrators.Part1;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.TrainingProvider;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.TrainingProvider;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Orchestrators.Part1.TrainingProviderOrchestratorTests;

public class WhenPostingSelectTrainingProvider
{
    [Test, MoqAutoData]
    public async Task Then_The_Supplying_Provider_Is_Found_When_Not_Using_A_LarsCode(
        Vacancy vacancy,
        SelectTrainingProviderEditModel model,
        List<TrainingProviderSummary> providerSummaries,
        TrainingProvider trainingProvider,
        [Frozen] Mock<IUtility> utility,
        [Frozen] Mock<IRecruitVacancyClient> vacancyClient,
        [Frozen] Mock<ITrainingProviderService> trainingProviderService,
        [Frozen] Mock<ITrainingProviderSummaryProvider> trainingProviderSummaryProvider,
        [Greedy] TrainingProviderOrchestrator sut)
    {
        // arrange
        model.SelectionType = TrainingProviderSelectionType.TrainingProviderSearch;
        vacancy.ProgrammeId = "100-1";
        var provider = providerSummaries.First();
        utility
            .Setup(x => x.GetAuthorisedVacancyForEditAsync(model, It.IsAny<string>()))
            .ReturnsAsync(vacancy);
        trainingProviderSummaryProvider
            .Setup(x => x.FindAllAsync())
            .ReturnsAsync(providerSummaries);
        trainingProviderService
            .Setup(x => x.GetProviderAsync(provider.Ukprn))
            .ReturnsAsync(trainingProvider);
        vacancyClient
            .Setup(x => x.Validate(vacancy, It.IsAny<VacancyRuleSet>()))
            .Returns(new EntityValidationResult());

        model.TrainingProviderSearch = $"{provider.ProviderName.ToUpper()} ({provider.Ukprn})";

        // act
        var result = await sut.PostSelectTrainingProviderAsync(model, null);

        // assert
        result.Success.Should().BeTrue();
        result.Data.FoundTrainingProviderUkprn.Should().Be(trainingProvider.Ukprn);
    }
    
    [Test, MoqAutoData]
    public async Task Then_The_Supplying_Provider_Is_Found_Using_A_LarsCode(
        Vacancy vacancy,
        SelectTrainingProviderEditModel model,
        List<TrainingProviderSummary> providerSummaries,
        TrainingProvider trainingProvider,
        [Frozen] Mock<IUtility> utility,
        [Frozen] Mock<IRecruitVacancyClient> vacancyClient,
        [Frozen] Mock<ITrainingProviderService> trainingProviderService,
        [Greedy] TrainingProviderOrchestrator sut)
    {
        // arrange
        model.SelectionType = TrainingProviderSelectionType.TrainingProviderSearch;
        vacancy.ProgrammeId = "100";
        var provider = providerSummaries.First();
        utility
            .Setup(x => x.GetAuthorisedVacancyForEditAsync(model, It.IsAny<string>()))
            .ReturnsAsync(vacancy);
        trainingProviderService
            .Setup(x => x.GetCourseProviders(100))
            .ReturnsAsync(providerSummaries);
        trainingProviderService
            .Setup(x => x.GetProviderAsync(provider.Ukprn))
            .ReturnsAsync(trainingProvider);
        vacancyClient
            .Setup(x => x.Validate(vacancy, It.IsAny<VacancyRuleSet>()))
            .Returns(new EntityValidationResult());

        model.TrainingProviderSearch = $"{provider.ProviderName.ToUpper()} ({provider.Ukprn})";

        // act
        var result = await sut.PostSelectTrainingProviderAsync(model, null);

        // assert
        result.Success.Should().BeTrue();
        result.Data.FoundTrainingProviderUkprn.Should().Be(trainingProvider.Ukprn);
    }
    
    [Test, MoqAutoData]
    public async Task Then_The_Supplying_Provider_Is_Not_Found(
        Vacancy vacancy,
        SelectTrainingProviderEditModel model,
        List<TrainingProviderSummary> providerSummaries,
        TrainingProvider trainingProvider,
        [Frozen] Mock<IUtility> utility,
        [Frozen] Mock<IRecruitVacancyClient> vacancyClient,
        [Frozen] Mock<ITrainingProviderService> trainingProviderService,
        [Greedy] TrainingProviderOrchestrator sut)
    {
        // arrange
        model.SelectionType = TrainingProviderSelectionType.TrainingProviderSearch;
        vacancy.ProgrammeId = "100";
        var provider = providerSummaries.First();
        utility
            .Setup(x => x.GetAuthorisedVacancyForEditAsync(model, It.IsAny<string>()))
            .ReturnsAsync(vacancy);
        trainingProviderService
            .Setup(x => x.GetCourseProviders(100))
            .ReturnsAsync(providerSummaries);
        trainingProviderService
            .Setup(x => x.GetProviderAsync(provider.Ukprn))
            .ReturnsAsync(trainingProvider);
        vacancyClient
            .Setup(x => x.Validate(vacancy, It.IsAny<VacancyRuleSet>()))
            .Returns(new EntityValidationResult());

        // act
        var result = await sut.PostSelectTrainingProviderAsync(model, null);

        // assert
        result.Success.Should().BeTrue();
        result.Data.FoundTrainingProviderUkprn.Should().BeNull();
    }
}