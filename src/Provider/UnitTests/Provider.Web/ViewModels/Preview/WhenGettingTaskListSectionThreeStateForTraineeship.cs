using System;
using System.Collections.Generic;
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
    public class WhenGettingTaskListSectionThreeStateForTraineeship
    {
        [Test, MoqAutoData]
        public async Task Then_The_Section_State_Is_Not_Started_When_Section_Two_InProgress(
            Mock<IRecruitVacancyClient> recruitVacancyClient,
            DisplayVacancyViewModelMapper mapper)
        {
            var vacancy = GetVacancySectionOneAndTwoComplete(recruitVacancyClient);
            vacancy.NumberOfPositions = null;
            var model = new VacancyPreviewViewModel();
            
            await mapper.MapFromVacancyAsync(model, vacancy);
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionOneState.Should().Be(VacancyTaskListSectionState.Completed);
            model.TaskListSectionTwoState.Should().Be(VacancyTaskListSectionState.InProgress);
            model.TaskListSectionThreeState.Should().Be(VacancyTaskListSectionState.NotStarted);
        }
        
        [Test, MoqAutoData]
        public async Task Then_The_Section_State_Is_Not_Started_When_Section_Two_Complete(
            Mock<IRecruitVacancyClient> recruitVacancyClient,
            DisplayVacancyViewModelMapper mapper)
        {
            var vacancy = GetVacancySectionOneAndTwoComplete(recruitVacancyClient);
            var model = new VacancyPreviewViewModel();
            
            await mapper.MapFromVacancyAsync(model, vacancy);
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionOneState.Should().Be(VacancyTaskListSectionState.Completed);
            model.TaskListSectionTwoState.Should().Be(VacancyTaskListSectionState.Completed);
            model.TaskListSectionThreeState.Should().Be(VacancyTaskListSectionState.NotStarted);
        }
        
        
        [Test, MoqAutoData]
        public async Task Then_The_Section_State_Is_InProgress_When_Has_Skills(
            List<string> skills,
            Mock<IRecruitVacancyClient> recruitVacancyClient,
            DisplayVacancyViewModelMapper mapper)
        {
            var vacancy = GetVacancySectionOneAndTwoComplete(recruitVacancyClient);
            vacancy.Skills = skills;
            
            var model = new VacancyPreviewViewModel();
            
            await mapper.MapFromVacancyAsync(model, vacancy);
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionOneState.Should().Be(VacancyTaskListSectionState.Completed);
            model.TaskListSectionTwoState.Should().Be(VacancyTaskListSectionState.Completed);
            model.TaskListSectionThreeState.Should().Be(VacancyTaskListSectionState.InProgress);
        }
        
        [Test, MoqAutoData]
        public async Task Then_The_Section_State_Is_Completed_When_Has_Skills_FutureProspects(
            List<string> skills,
            string outcomeDescription,
            Mock<IRecruitVacancyClient> recruitVacancyClient,
            DisplayVacancyViewModelMapper mapper)
        {
            var vacancy = GetVacancySectionOneAndTwoComplete(recruitVacancyClient);
            vacancy.Skills = skills;
            vacancy.OutcomeDescription = outcomeDescription;
            
            var model = new VacancyPreviewViewModel();
            
            await mapper.MapFromVacancyAsync(model, vacancy);
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionOneState.Should().Be(VacancyTaskListSectionState.Completed);
            model.TaskListSectionTwoState.Should().Be(VacancyTaskListSectionState.Completed);
            model.TaskListSectionThreeState.Should().Be(VacancyTaskListSectionState.Completed);
        }
        
        [Test, MoqAutoData]
        public async Task Then_The_Section_State_Is_Complete_When_Has_Skills_FutureProspects_ThingsToConsider(
            List<string> skills,
            string futureProspects,
            string thingsToConsider,
            string outcomeDescription,
            Mock<IRecruitVacancyClient> recruitVacancyClient,
            DisplayVacancyViewModelMapper mapper)
        {
            var vacancy = GetVacancySectionOneAndTwoComplete(recruitVacancyClient);
            vacancy.Skills = skills;
            vacancy.OutcomeDescription = outcomeDescription;
            vacancy.ThingsToConsider = thingsToConsider;
            
            var model = new VacancyPreviewViewModel();
            
            await mapper.MapFromVacancyAsync(model, vacancy);
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionOneState.Should().Be(VacancyTaskListSectionState.Completed);
            model.TaskListSectionTwoState.Should().Be(VacancyTaskListSectionState.Completed);
            model.TaskListSectionThreeState.Should().Be(VacancyTaskListSectionState.Completed);
        }
        
        private Vacancy GetVacancySectionOneAndTwoComplete(Mock<IRecruitVacancyClient> recruitVacancyClient)
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
                TrainingDescription = fixture.Create<string>(),
                ShortDescription = fixture.Create<string>(),
                AccountLegalEntityPublicHashedId = fixture.Create<string>(),
                StartDate = DateTime.UtcNow.AddMonths(3),
                ClosingDate = DateTime.UtcNow.AddMonths(2),
                WorkExperience = fixture.Create<string>(),
                Wage = fixture.Create<Wage>(),
                EmployerLocation = fixture.Create<Address>(),
                NumberOfPositions = fixture.Create<int>(),
                VacancyType = VacancyType.Traineeship
            };
        }
    }
}