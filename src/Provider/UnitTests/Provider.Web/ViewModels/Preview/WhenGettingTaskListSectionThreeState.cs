using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.NUnit3;
using Esfa.Recruit.Provider.Web.Mappings;
using Esfa.Recruit.Provider.Web.ViewModels.VacancyPreview;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Moq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.ViewModels.Preview
{
    public class WhenGettingTaskListSectionThreeState
    {
        [Test, MoqAutoData]
        public async Task Then_The_Section_State_Is_Not_Started_When_Section_Two_InProgress(
            [Frozen] Mock<IApprenticeshipProgrammeProvider> apprenticeshipProgrammeProvider,
            DisplayVacancyViewModelMapper mapper)
        {
            var vacancy = GetVacancySectionOneAndTwoComplete(apprenticeshipProgrammeProvider);
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
            [Frozen] Mock<IApprenticeshipProgrammeProvider> apprenticeshipProgrammeProvider,
            DisplayVacancyViewModelMapper mapper)
        {
            var vacancy = GetVacancySectionOneAndTwoComplete(apprenticeshipProgrammeProvider);
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
            [Frozen] Mock<IApprenticeshipProgrammeProvider> apprenticeshipProgrammeProvider,
            DisplayVacancyViewModelMapper mapper)
        {
            var vacancy = GetVacancySectionOneAndTwoComplete(apprenticeshipProgrammeProvider);
            vacancy.Skills = skills;
            var model = new VacancyPreviewViewModel();
            
            await mapper.MapFromVacancyAsync(model, vacancy);
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionOneState.Should().Be(VacancyTaskListSectionState.Completed);
            model.TaskListSectionTwoState.Should().Be(VacancyTaskListSectionState.Completed);
            model.TaskListSectionThreeState.Should().Be(VacancyTaskListSectionState.InProgress);
        }
        
        [Test, MoqAutoData]
        public async Task Then_The_Section_State_Is_InProgress_When_Has_Skills_Qualifications(
            List<string> skills,
            List<Qualification> qualifications,
            [Frozen] Mock<IApprenticeshipProgrammeProvider> apprenticeshipProgrammeProvider,
            DisplayVacancyViewModelMapper mapper)
        {
            var vacancy = GetVacancySectionOneAndTwoComplete(apprenticeshipProgrammeProvider);
            vacancy.Skills = skills;
            vacancy.Qualifications = qualifications;
            
            var model = new VacancyPreviewViewModel();
            
            await mapper.MapFromVacancyAsync(model, vacancy);
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionOneState.Should().Be(VacancyTaskListSectionState.Completed);
            model.TaskListSectionTwoState.Should().Be(VacancyTaskListSectionState.Completed);
            model.TaskListSectionThreeState.Should().Be(VacancyTaskListSectionState.InProgress);
        }
        
        [Test, MoqAutoData]
        public async Task Then_If_There_Are_Skills_And_Opted_Not_To_Add_Qualifications_And_FutureProspects_Added_Section_Set_To_Complete(
            string outcomeDescription,
            List<string> skills,
            List<Qualification> qualifications,
            [Frozen] Mock<IApprenticeshipProgrammeProvider> apprenticeshipProgrammeProvider,
            DisplayVacancyViewModelMapper mapper)
        {
            var vacancy = GetVacancySectionOneAndTwoComplete(apprenticeshipProgrammeProvider);
            vacancy.Skills = skills;
            vacancy.Qualifications = null;
            vacancy.OutcomeDescription = outcomeDescription;
            vacancy.HasOptedToAddQualifications = false;
            var model = new VacancyPreviewViewModel();
            
            await mapper.MapFromVacancyAsync(model, vacancy);
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionThreeState.Should().Be(VacancyTaskListSectionState.Completed);
        }
        
        
        [Test, MoqAutoData]
        public async Task Then_The_Section_State_Is_Completed_When_Has_Skills_Qualifications_FutureProspects(
            List<string> skills,
            List<Qualification> qualifications,
            string outcomeDescription,
            [Frozen] Mock<IApprenticeshipProgrammeProvider> apprenticeshipProgrammeProvider,
            DisplayVacancyViewModelMapper mapper)
        {
            var vacancy = GetVacancySectionOneAndTwoComplete(apprenticeshipProgrammeProvider);
            vacancy.Skills = skills;
            vacancy.Qualifications = qualifications;
            vacancy.OutcomeDescription = outcomeDescription;
            
            var model = new VacancyPreviewViewModel();
            
            await mapper.MapFromVacancyAsync(model, vacancy);
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionOneState.Should().Be(VacancyTaskListSectionState.Completed);
            model.TaskListSectionTwoState.Should().Be(VacancyTaskListSectionState.Completed);
            model.TaskListSectionThreeState.Should().Be(VacancyTaskListSectionState.Completed);
        }
        
        [Test, MoqAutoData]
        public async Task Then_The_Section_State_Is_Complete_When_Has_Skills_Qualifications_FutureProspects_ThingsToConsider(
            List<string> skills,
            List<Qualification> qualifications,
            string futureProspects,
            string thingsToConsider,
            string outcomeDescription,
            [Frozen] Mock<IApprenticeshipProgrammeProvider> apprenticeshipProgrammeProvider,
            DisplayVacancyViewModelMapper mapper)
        {
            var vacancy = GetVacancySectionOneAndTwoComplete(apprenticeshipProgrammeProvider);
            vacancy.Skills = skills;
            vacancy.Qualifications = qualifications;
            vacancy.OutcomeDescription = outcomeDescription;
            vacancy.ThingsToConsider = thingsToConsider;
            
            var model = new VacancyPreviewViewModel();
            
            await mapper.MapFromVacancyAsync(model, vacancy);
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionOneState.Should().Be(VacancyTaskListSectionState.Completed);
            model.TaskListSectionTwoState.Should().Be(VacancyTaskListSectionState.Completed);
            model.TaskListSectionThreeState.Should().Be(VacancyTaskListSectionState.Completed);
        }
        
        private Vacancy GetVacancySectionOneAndTwoComplete(Mock<IApprenticeshipProgrammeProvider> apprenticeshipProgrammeProvider)
        {
            var fixture = new Fixture();
            var programmeId = fixture.Create<int>();
            var standard = fixture.Create<ApprenticeshipStandard>();
            apprenticeshipProgrammeProvider.Setup(x => x.GetApprenticeshipStandardVacancyPreviewData(programmeId))
                .ReturnsAsync(standard);
            
            return new Vacancy
            {
                Id = Guid.NewGuid(),
                TrainingProvider = fixture.Create<TrainingProvider>(),
                Title = fixture.Create<string>(),
                ProgrammeId = programmeId.ToString(),
                Description = fixture.Create<string>(),
                TrainingDescription = fixture.Create<string>(),
                ShortDescription = fixture.Create<string>(),
                AccountLegalEntityPublicHashedId = fixture.Create<string>(),
                StartDate = DateTime.UtcNow.AddMonths(3),
                ClosingDate = DateTime.UtcNow.AddMonths(2),
                Wage = fixture.Create<Wage>(),
                EmployerLocation = fixture.Create<Address>(),
                NumberOfPositions = fixture.Create<int>(),
            };
        }
    }
}