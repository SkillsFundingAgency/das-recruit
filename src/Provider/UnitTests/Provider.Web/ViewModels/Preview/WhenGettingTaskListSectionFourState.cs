using Esfa.Recruit.Provider.Web.Mappings;
using Esfa.Recruit.Provider.Web.ViewModels.VacancyPreview;
using Esfa.Recruit.Shared.Web.ViewModels.TaskList;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using NUnit.Framework;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.ViewModels.Preview
{
    public class WhenGettingTaskListSectionFourState
    {
        [Test, MoqAutoData]
        public async Task Then_The_Section_State_Is_Not_Started_When_Section_Three_InProgress(
            Vacancy vacancy,
            [Frozen] Mock<IRecruitVacancyClient> recruitVacancyClient,
            [Frozen] Mock<IApprenticeshipProgrammeProvider> apprenticeshipProgrammeProvider,
            DisplayVacancyViewModelMapper mapper)
        {
            SetApprenticeshipStandardOnVacancy(apprenticeshipProgrammeProvider, vacancy);
            recruitVacancyClient.Setup(x => x.GetEmployerNameAsync(vacancy)).ReturnsAsync(string.Empty);
            vacancy.EmployerDescription = null;
            vacancy.ApplicationMethod = null;
            vacancy.OutcomeDescription = "";
            vacancy.EmployerNameOption = null;
            var model = new VacancyPreviewViewModel();
            
            await mapper.MapFromVacancyAsync(model, vacancy);
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionOneState.Should().Be(VacancyTaskListSectionState.Completed);
            model.TaskListSectionTwoState.Should().Be(VacancyTaskListSectionState.Completed);
            model.TaskListSectionThreeState.Should().Be(VacancyTaskListSectionState.InProgress);
            model.TaskListSectionFourState.Should().Be(VacancyTaskListSectionState.NotStarted);
        }
        
        [Test, MoqAutoData]
        public async Task Then_The_Section_State_Is_Not_Started_When_Section_Three_Completed(
            Vacancy vacancy,
            [Frozen] Mock<IRecruitVacancyClient> recruitVacancyClient,
            [Frozen] Mock<IApprenticeshipProgrammeProvider> apprenticeshipProgrammeProvider,
            DisplayVacancyViewModelMapper mapper)
        {
            SetApprenticeshipStandardOnVacancy(apprenticeshipProgrammeProvider, vacancy);
            recruitVacancyClient.Setup(x => x.GetEmployerNameAsync(vacancy)).ReturnsAsync(string.Empty);
            vacancy.EmployerDescription = null;
            vacancy.ApplicationMethod = null;
            vacancy.EmployerNameOption = null;
            var model = new VacancyPreviewViewModel();
            
            await mapper.MapFromVacancyAsync(model, vacancy);
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionOneState.Should().Be(VacancyTaskListSectionState.Completed);
            model.TaskListSectionTwoState.Should().Be(VacancyTaskListSectionState.Completed);
            model.TaskListSectionThreeState.Should().Be(VacancyTaskListSectionState.Completed);
            model.TaskListSectionFourState.Should().Be(VacancyTaskListSectionState.NotStarted);
        }
        
        [Test, MoqAutoData]
        public async Task Then_The_Section_State_Is_In_Progress_When_Has_EmployerNameOption(
            Vacancy vacancy,
            [Frozen] Mock<IRecruitVacancyClient> recruitVacancyClient,
            [Frozen] Mock<IApprenticeshipProgrammeProvider> apprenticeshipProgrammeProvider,
            DisplayVacancyViewModelMapper mapper)
        {
            SetApprenticeshipStandardOnVacancy(apprenticeshipProgrammeProvider, vacancy);
            recruitVacancyClient.Setup(x => x.GetEmployerNameAsync(vacancy)).ReturnsAsync(string.Empty);
            vacancy.EmployerDescription = null;
            vacancy.ApplicationMethod = null;
            var model = new VacancyPreviewViewModel();
            
            await mapper.MapFromVacancyAsync(model, vacancy);
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionOneState.Should().Be(VacancyTaskListSectionState.Completed);
            model.TaskListSectionTwoState.Should().Be(VacancyTaskListSectionState.Completed);
            model.TaskListSectionThreeState.Should().Be(VacancyTaskListSectionState.Completed);
            model.TaskListSectionFourState.Should().Be(VacancyTaskListSectionState.InProgress);
        }
        
        [Test, MoqAutoData]
        public async Task Then_The_Section_State_Is_In_Progress_When_Has_EmployerNameOption_And_Employer_Description(
            Vacancy vacancy,
            [Frozen] Mock<IRecruitVacancyClient> recruitVacancyClient,
            [Frozen] Mock<IApprenticeshipProgrammeProvider> apprenticeshipProgrammeProvider,
            DisplayVacancyViewModelMapper mapper)
        {
            SetApprenticeshipStandardOnVacancy(apprenticeshipProgrammeProvider, vacancy);
            recruitVacancyClient.Setup(x => x.GetEmployerNameAsync(vacancy)).ReturnsAsync(string.Empty);
            vacancy.ApplicationMethod = null;
            var model = new VacancyPreviewViewModel();
            
            await mapper.MapFromVacancyAsync(model, vacancy);
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionOneState.Should().Be(VacancyTaskListSectionState.Completed);
            model.TaskListSectionTwoState.Should().Be(VacancyTaskListSectionState.Completed);
            model.TaskListSectionThreeState.Should().Be(VacancyTaskListSectionState.Completed);
            model.TaskListSectionFourState.Should().Be(VacancyTaskListSectionState.InProgress);
        }
        
        [Test, MoqAutoData]
        public async Task Then_The_Section_State_Is_Completed_When_Has_EmployerNameOption_Employer_Description_And_ApplicationMethod(
            Vacancy vacancy,
            int programmeId,
            ApprenticeshipStandard standard,
            [Frozen] Mock<IApprenticeshipProgrammeProvider> apprenticeshipProgrammeProvider,
            [Frozen] Mock<IRecruitVacancyClient> recruitVacancyClient,
            DisplayVacancyViewModelMapper mapper)
        {

            SetApprenticeshipStandardOnVacancy(apprenticeshipProgrammeProvider, vacancy);
            recruitVacancyClient.Setup(x => x.GetEmployerNameAsync(vacancy)).ReturnsAsync(string.Empty);
            
            var model = new VacancyPreviewViewModel();
            
            await mapper.MapFromVacancyAsync(model, vacancy);
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionOneState.Should().Be(VacancyTaskListSectionState.Completed);
            model.TaskListSectionTwoState.Should().Be(VacancyTaskListSectionState.Completed);
            model.TaskListSectionThreeState.Should().Be(VacancyTaskListSectionState.Completed);
            model.TaskListSectionFourState.Should().Be(VacancyTaskListSectionState.Completed);
        }

        private void SetApprenticeshipStandardOnVacancy(Mock<IApprenticeshipProgrammeProvider> apprenticeshipProgrammeProvider, Vacancy vacancy)
        {
            var fixture = new Fixture();
            var programmeId = fixture.Create<int>();
            var standard = fixture.Create<ApprenticeshipStandard>();
            apprenticeshipProgrammeProvider.Setup(x => x.GetApprenticeshipStandardVacancyPreviewData(programmeId))
                .ReturnsAsync(standard);
            vacancy.ProgrammeId = programmeId.ToString();
        }
    }
}