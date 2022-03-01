using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.ViewModels.VacancyPreview;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Moq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.ViewModels.VacancyPreview
{
    public class WhenBuildingVacancyPreviewViewModelSectionFourState
    {
        [Test, MoqAutoData]
        public async Task Then_The_Section_State_Is_Set_To_Not_Started_If_Section_Three_Is_Not_Complete(
            Vacancy vacancy,
            [Frozen] Mock<IRecruitVacancyClient> recruitVacancyClient,
            DisplayVacancyViewModelMapper mapper)
        {
            recruitVacancyClient.Setup(x => x.GetEmployerNameAsync(vacancy)).ReturnsAsync(string.Empty);
            vacancy.EmployerDescription = null;
            vacancy.ApplicationMethod = null;
            vacancy.Skills = null;
            
            var model = new VacancyPreviewViewModel();
            
            await mapper.MapFromVacancyAsync(model, vacancy);
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionFourState.Should().Be(VacancyTaskListSectionState.NotStarted);
        }
        
        [Test, MoqAutoData]
        public async Task Then_The_Section_State_Is_Set_To_Not_Started_If_Section_Three_Is_Complete_But_No_Name_On_Advert(
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

            model.TaskListSectionFourState.Should().Be(VacancyTaskListSectionState.NotStarted);
        }

        [Test, MoqAutoData]
        public async Task Then_Section_State_Is_Set_To_In_Progress_If_Employer_Name_Set(
            Vacancy vacancy,
            [Frozen] Mock<IRecruitVacancyClient> recruitVacancyClient,
            DisplayVacancyViewModelMapper mapper)
        {
            vacancy.ApplicationMethod = null;
            vacancy.EmployerDescription = null;
            
            var model = new VacancyPreviewViewModel();
            
            await mapper.MapFromVacancyAsync(model, vacancy);
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionFourState.Should().Be(VacancyTaskListSectionState.InProgress);
        }
        
        
        [Test, MoqAutoData]
        public async Task Then_Section_State_Is_Set_To_In_Progress_If_Employer_Name_And_Description_Set(
            Vacancy vacancy,
            [Frozen] Mock<IRecruitVacancyClient> recruitVacancyClient,
            DisplayVacancyViewModelMapper mapper)
        {
            
            vacancy.ApplicationMethod = null;
            
            var model = new VacancyPreviewViewModel();
            
            await mapper.MapFromVacancyAsync(model, vacancy);
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionFourState.Should().Be(VacancyTaskListSectionState.InProgress);
        }

        [Test, MoqAutoData]
        public async Task Then_Section_State_Is_Set_To_Complete_With_Employer_Name_Description_And_ApplicationMethod_Set(
                Vacancy vacancy,
                [Frozen] Mock<IRecruitVacancyClient> recruitVacancyClient,
                DisplayVacancyViewModelMapper mapper)
        {
            vacancy.EmployerNameOption = EmployerNameOption.RegisteredName;
            var model = new VacancyPreviewViewModel();
            
            await mapper.MapFromVacancyAsync(model, vacancy);
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionFourState.Should().Be(VacancyTaskListSectionState.Completed);
        }
        
    }
}