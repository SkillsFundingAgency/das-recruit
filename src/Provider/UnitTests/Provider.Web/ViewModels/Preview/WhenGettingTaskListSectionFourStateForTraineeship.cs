using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Mappings;
using Esfa.Recruit.Provider.Web.ViewModels.VacancyPreview;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Moq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.ViewModels.Preview
{
    public class WhenGettingTaskListSectionFourStateForTraineeship
    {
        [Test, MoqAutoData]
        public async Task Then_The_Section_State_Is_Not_Started_When_Section_Three_InProgress(
            Vacancy vacancy,
            Mock<IRecruitVacancyClient> recruitVacancyClient,
            DisplayVacancyViewModelMapper mapper)
        {
            recruitVacancyClient.Setup(x => x.GetEmployerNameAsync(vacancy)).ReturnsAsync(string.Empty);
            vacancy.EmployerDescription = null;
            vacancy.ApplicationMethod = null;
            vacancy.OutcomeDescription = "";
            vacancy.EmployerNameOption = null;
            vacancy.VacancyType = VacancyType.Traineeship;
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
            Mock<IRecruitVacancyClient> recruitVacancyClient,
            DisplayVacancyViewModelMapper mapper)
        {
            recruitVacancyClient.Setup(x => x.GetEmployerNameAsync(vacancy)).ReturnsAsync(string.Empty);
            vacancy.EmployerDescription = null;
            vacancy.ApplicationMethod = null;
            vacancy.EmployerNameOption = null;
            vacancy.VacancyType = VacancyType.Traineeship;
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
            Mock<IRecruitVacancyClient> recruitVacancyClient,
            DisplayVacancyViewModelMapper mapper)
        {
            recruitVacancyClient.Setup(x => x.GetEmployerNameAsync(vacancy)).ReturnsAsync(string.Empty);
            vacancy.EmployerDescription = null;
            vacancy.ApplicationMethod = null;
            vacancy.VacancyType = VacancyType.Traineeship;
            var model = new VacancyPreviewViewModel();
            
            await mapper.MapFromVacancyAsync(model, vacancy);
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionOneState.Should().Be(VacancyTaskListSectionState.Completed);
            model.TaskListSectionTwoState.Should().Be(VacancyTaskListSectionState.Completed);
            model.TaskListSectionThreeState.Should().Be(VacancyTaskListSectionState.Completed);
            model.TaskListSectionFourState.Should().Be(VacancyTaskListSectionState.InProgress);
        }
        
        [Test, MoqAutoData]
        public async Task Then_The_Section_State_Is_In_Completed_When_Has_EmployerNameOption_And_Employer_Description(
            Vacancy vacancy,
            Mock<IRecruitVacancyClient> recruitVacancyClient,
            DisplayVacancyViewModelMapper mapper)
        {
            recruitVacancyClient.Setup(x => x.GetEmployerNameAsync(vacancy)).ReturnsAsync(string.Empty);
            vacancy.ApplicationMethod = null;
            vacancy.VacancyType = VacancyType.Traineeship;
            var model = new VacancyPreviewViewModel();
            
            await mapper.MapFromVacancyAsync(model, vacancy);
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionOneState.Should().Be(VacancyTaskListSectionState.Completed);
            model.TaskListSectionTwoState.Should().Be(VacancyTaskListSectionState.Completed);
            model.TaskListSectionThreeState.Should().Be(VacancyTaskListSectionState.Completed);
            model.TaskListSectionFourState.Should().Be(VacancyTaskListSectionState.Completed);
        }
    }
}