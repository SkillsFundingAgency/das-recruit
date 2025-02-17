using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Provider.Web.Mappings;
using Esfa.Recruit.Provider.Web.ViewModels.VacancyPreview;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Moq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.ViewModels.Preview;

public class WhenBuildingVacancyPreviewViewModelSectionFiveState
{
    [Test, MoqAutoData]
    public async Task Then_The_Section_State_Is_Set_To_Not_Started_If_Section_Four_Is_Not_Complete(
        Vacancy vacancy,
        int programmeId,
        ApprenticeshipStandard standard,
        [Frozen] Mock<IApprenticeshipProgrammeProvider> apprenticeshipProgrammeProvider,
        [Frozen] Mock<IRecruitVacancyClient> recruitVacancyClient,
        DisplayVacancyViewModelMapper mapper)
    {
        apprenticeshipProgrammeProvider.Setup(x => x.GetApprenticeshipStandardVacancyPreviewData(programmeId))
            .ReturnsAsync(standard);
        vacancy.ProgrammeId = programmeId.ToString();
        recruitVacancyClient.Setup(x => x.GetEmployerNameAsync(vacancy)).ReturnsAsync(string.Empty);
        vacancy.EmployerDescription = null;
        vacancy.ApplicationMethod = null;
        vacancy.EmployerNameOption = null;

        var model = new VacancyPreviewViewModel();

        await mapper.MapFromVacancyAsync(model, vacancy);
        model.SetSectionStates(model, new ModelStateDictionary());

        model.TaskListSectionFiveState.Should().Be(VacancyTaskListSectionState.NotStarted);
    }

    [Test, MoqAutoData]
    public async Task Then_The_Section_State_Is_Set_To_Not_Started_If_Section_Four_Is_Complete_But_Additional_Questions_Not_Saved(
        Vacancy vacancy,
        int programmeId,
        ApprenticeshipStandard standard,
        [Frozen] Mock<IApprenticeshipProgrammeProvider> apprenticeshipProgrammeProvider,
        [Frozen] Mock<IRecruitVacancyClient> recruitVacancyClient,
        DisplayVacancyViewModelMapper mapper)
    {
        apprenticeshipProgrammeProvider.Setup(x => x.GetApprenticeshipStandardVacancyPreviewData(programmeId))
            .ReturnsAsync(standard);
        vacancy.ProgrammeId = programmeId.ToString();
        vacancy.HasSubmittedAdditionalQuestions = false;

        var model = new VacancyPreviewViewModel();

        await mapper.MapFromVacancyAsync(model, vacancy);
        model.SetSectionStates(model, new ModelStateDictionary());

        model.TaskListSectionFiveState.Should().Be(VacancyTaskListSectionState.NotStarted);
    }

    [Test, MoqAutoData]
    public async Task Then_The_Section_State_Is_Set_To_Completed_If_Additional_Questions_Saved(
        Vacancy vacancy,
        int programmeId,
        ApprenticeshipStandard standard,
        [Frozen] Mock<IRecruitVacancyClient> recruitVacancyClient,
        [Frozen] Mock<IApprenticeshipProgrammeProvider> apprenticeshipProgrammeProvider,
        DisplayVacancyViewModelMapper mapper)
    {
        apprenticeshipProgrammeProvider.Setup(x => x.GetApprenticeshipStandardVacancyPreviewData(programmeId))
            .ReturnsAsync(standard);
        vacancy.ProgrammeId = programmeId.ToString();
        vacancy.HasSubmittedAdditionalQuestions = true;

        var model = new VacancyPreviewViewModel();

        await mapper.MapFromVacancyAsync(model, vacancy);
        model.SetSectionStates(model, new ModelStateDictionary());

        model.TaskListSectionFiveState.Should().Be(VacancyTaskListSectionState.Completed);
    }
}