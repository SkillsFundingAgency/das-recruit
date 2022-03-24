using System;
using System.Threading.Tasks;
using AutoFixture;
using Esfa.Recruit.Provider.Web.Mappings;
using Esfa.Recruit.Provider.Web.ViewModels.VacancyPreview;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.ApprenticeshipProgrammes;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Moq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.ViewModels.Preview
{
    public class WhenGettingTaskListSectionTwoState
    {
        [Test, MoqAutoData]
        public async Task Then_The_Section_State_Is_Not_Started_When_Section_One_Complete(
            Mock<IRecruitVacancyClient> recruitVacancyClient,
            DisplayVacancyViewModelMapper mapper)
        {
            var vacancy = GetVacancySectionOneComplete(recruitVacancyClient);
            
            var model = new VacancyPreviewViewModel();
            
            await mapper.MapFromVacancyAsync(model, vacancy);
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionOneState.Should().Be(VacancyTaskListSectionState.Completed);
            model.TaskListSectionTwoState.Should().Be(VacancyTaskListSectionState.NotStarted);
        }
        
        [Test, MoqAutoData]
        public async Task Then_The_Section_State_Is_Not_Started_When_Section_One_InProgress(
            Mock<IRecruitVacancyClient> recruitVacancyClient,
            DisplayVacancyViewModelMapper mapper)
        {
            var vacancy = GetVacancySectionOneComplete(recruitVacancyClient);
            vacancy.ShortDescription = "";
            
            var model = new VacancyPreviewViewModel();
            
            await mapper.MapFromVacancyAsync(model, vacancy);
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionOneState.Should().Be(VacancyTaskListSectionState.InProgress);
            model.TaskListSectionTwoState.Should().Be(VacancyTaskListSectionState.NotStarted);
        }

        [Test, MoqAutoData]
        public async Task Then_The_Section_Is_Set_To_In_Progress_When_Dates_Set(
            Mock<IRecruitVacancyClient> recruitVacancyClient,
            DisplayVacancyViewModelMapper mapper)
        {
            var vacancy = GetVacancySectionOneComplete(recruitVacancyClient);
            vacancy.StartDate = DateTime.UtcNow.AddMonths(3);
            vacancy.ClosingDate = DateTime.UtcNow.AddMonths(2);
            var model = new VacancyPreviewViewModel();
            
            await mapper.MapFromVacancyAsync(model, vacancy);
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionOneState.Should().Be(VacancyTaskListSectionState.Completed);
            model.TaskListSectionTwoState.Should().Be(VacancyTaskListSectionState.InProgress);
        }
        
        [Test, MoqAutoData]
        public async Task Then_The_Section_Is_Set_To_In_Progress_When_Dates_Duration_Set(
            Mock<IRecruitVacancyClient> recruitVacancyClient,
            DisplayVacancyViewModelMapper mapper)
        {
            var vacancy = GetVacancySectionOneComplete(recruitVacancyClient);
            vacancy.StartDate = DateTime.UtcNow.AddMonths(3);
            vacancy.ClosingDate = DateTime.UtcNow.AddMonths(2);
            
            vacancy.Wage = new Wage
            {
                Duration = 36,
                DurationUnit = DurationUnit.Month,
                WeeklyHours = 30
            }; 
            var model = new VacancyPreviewViewModel();
            
            await mapper.MapFromVacancyAsync(model, vacancy);
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionOneState.Should().Be(VacancyTaskListSectionState.Completed);
            model.TaskListSectionTwoState.Should().Be(VacancyTaskListSectionState.InProgress);
        }
        
        [Test, MoqAutoData]
        public async Task Then_The_Section_Is_Set_To_In_Progress_When_Dates_Duration_Wage_Set(
            Wage wage,
            Mock<IRecruitVacancyClient> recruitVacancyClient,
            DisplayVacancyViewModelMapper mapper)
        {
            var vacancy = GetVacancySectionOneComplete(recruitVacancyClient);
            vacancy.StartDate = DateTime.UtcNow.AddMonths(3);
            vacancy.ClosingDate = DateTime.UtcNow.AddMonths(2);
            vacancy.Wage = wage; 
            var model = new VacancyPreviewViewModel();
            
            await mapper.MapFromVacancyAsync(model, vacancy);
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionOneState.Should().Be(VacancyTaskListSectionState.Completed);
            model.TaskListSectionTwoState.Should().Be(VacancyTaskListSectionState.InProgress);
        }
        
        [Test, MoqAutoData]
        public async Task Then_The_Section_Is_Set_To_In_Progress_When_Dates_Duration_Wage_Number_Of_Positions_Set(
            Wage wage,
            Mock<IRecruitVacancyClient> recruitVacancyClient,
            DisplayVacancyViewModelMapper mapper)
        {
            var vacancy = GetVacancySectionOneComplete(recruitVacancyClient);
            vacancy.StartDate = DateTime.UtcNow.AddMonths(3);
            vacancy.ClosingDate = DateTime.UtcNow.AddMonths(2);
            vacancy.Wage = wage; 
            var model = new VacancyPreviewViewModel();
            
            await mapper.MapFromVacancyAsync(model, vacancy);
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionOneState.Should().Be(VacancyTaskListSectionState.Completed);
            model.TaskListSectionTwoState.Should().Be(VacancyTaskListSectionState.InProgress);
        }
        
        [Test, MoqAutoData]
        public async Task Then_The_Section_Is_Set_To_Complete_When_Dates_Duration_Wage_Number_Of_Positions_address_Set(
            int? numberOfPositions,
            Wage wage,
            Address address,
            Mock<IRecruitVacancyClient> recruitVacancyClient,
            DisplayVacancyViewModelMapper mapper)
        {
            var vacancy = GetVacancySectionOneComplete(recruitVacancyClient);
            vacancy.StartDate = DateTime.UtcNow.AddMonths(3);
            vacancy.ClosingDate = DateTime.UtcNow.AddMonths(2);
            vacancy.Wage = wage;
            vacancy.EmployerLocation = address;
            vacancy.NumberOfPositions = numberOfPositions;
            var model = new VacancyPreviewViewModel();
            
            await mapper.MapFromVacancyAsync(model, vacancy);
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionOneState.Should().Be(VacancyTaskListSectionState.Completed);
            model.TaskListSectionTwoState.Should().Be(VacancyTaskListSectionState.Completed);
        }

        private Vacancy GetVacancySectionOneComplete(Mock<IRecruitVacancyClient> recruitVacancyClient)
        {
            var fixture = new Fixture();
            var programmeId = fixture.Create<string>();
            var programme = fixture.Create<ApprenticeshipProgramme>();
            recruitVacancyClient.Setup(x => x.GetApprenticeshipProgrammeAsync(programmeId)).ReturnsAsync(programme);
            
            return new Vacancy
            {
                Id = Guid.NewGuid(),
                TrainingProvider = fixture.Create<TrainingProvider>(),
                Title = fixture.Create<string>(),
                ProgrammeId = programmeId,
                Description = fixture.Create<string>(),
                TrainingDescription = fixture.Create<string>(),
                ShortDescription = fixture.Create<string>(),
                AccountLegalEntityPublicHashedId = fixture.Create<string>()
            };
        }
    }
}