﻿using AutoFixture.NUnit3;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.ViewModels.VacancyPreview;
using Esfa.Recruit.Shared.Web.ViewModels.TaskList;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.ViewModels.VacancyPreview;

public class WhenBuildingVacancyPreviewViewModelSectionFiveState
{
    [Test, MoqAutoData]
    public async Task Then_The_Section_State_Is_Set_To_Not_Started_If_Section_Four_Is_Not_Complete(
        Vacancy vacancy,
        [Frozen] Mock<IRecruitVacancyClient> recruitVacancyClient,
        DisplayVacancyViewModelMapper mapper)
    {
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
        [Frozen] Mock<IRecruitVacancyClient> recruitVacancyClient,
        DisplayVacancyViewModelMapper mapper)
    {
        vacancy.HasSubmittedAdditionalQuestions = false;

        var model = new VacancyPreviewViewModel();

        await mapper.MapFromVacancyAsync(model, vacancy);
        model.SetSectionStates(model, new ModelStateDictionary());

        model.TaskListSectionFiveState.Should().Be(VacancyTaskListSectionState.NotStarted);
    }

    [Test, MoqAutoData]
    public async Task Then_The_Section_State_Is_Set_To_Completed_If_Additional_Questions_Saved(
        int standardId,
        ApprenticeshipStandard standard,
        Vacancy vacancy,
        [Frozen] Mock<IApprenticeshipProgrammeProvider> apprenticeshipProgrammeProvider,
        [Frozen] Mock<IRecruitVacancyClient> recruitVacancyClient,
        DisplayVacancyViewModelMapper mapper)
    {
        apprenticeshipProgrammeProvider.Setup(x => x.GetApprenticeshipStandardVacancyPreviewData(standardId))
            .ReturnsAsync(standard);
        vacancy.ProgrammeId = standardId.ToString();
        vacancy.HasSubmittedAdditionalQuestions = true;

        var model = new VacancyPreviewViewModel();

        await mapper.MapFromVacancyAsync(model, vacancy);
        model.SetSectionStates(model, new ModelStateDictionary());

        model.TaskListSectionFiveState.Should().Be(VacancyTaskListSectionState.Completed);
    }
}