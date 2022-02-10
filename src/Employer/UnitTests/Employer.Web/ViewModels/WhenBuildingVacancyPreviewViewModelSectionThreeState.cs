using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.ViewModels.VacancyPreview;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.ApprenticeshipProgrammes;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Moq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.ViewModels
{
    public class WhenBuildingVacancyPreviewViewModelSectionFourState
    {
        [Test, MoqAutoData]
        public async Task Then_The_Section_State_Is_Set_To_Not_Started_If_No_Employer_Name(
            Vacancy vacancy,
            [Frozen] Mock<IRecruitVacancyClient> recruitVacancyClient,
            DisplayVacancyViewModelMapper mapper)
        {
            recruitVacancyClient.Setup(x => x.GetEmployerNameAsync(vacancy)).ReturnsAsync(string.Empty);
            vacancy.EmployerContact = null;
            vacancy.EmployerDescription = null;
            vacancy.ApplicationMethod = null;
            
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
            vacancy.EmployerContact = null;
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
            
            vacancy.EmployerContact = null;
            
            var model = new VacancyPreviewViewModel();
            
            await mapper.MapFromVacancyAsync(model, vacancy);
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionFourState.Should().Be(VacancyTaskListSectionState.InProgress);
        }
        
    }
    public class WhenBuildingVacancyPreviewViewModelSectionThreeState
    {
        [Test, MoqAutoData]
        public async Task Then_The_Section_State_Is_Set_to_Not_Started(
            string title,
            string programmeId,
            string description,
            string shortDescription,
            string trainingDescription,
            string outcomeDescription,
            Vacancies.Client.Domain.Entities.TrainingProvider provider,
            ApprenticeshipProgramme programme,
            [Frozen] Mock<IRecruitVacancyClient> recruitVacancyClient,
            DisplayVacancyViewModelMapper mapper)
        {
            var vacancy = new Vacancy
            {
                Id = Guid.NewGuid(),
                Title = title,
                ProgrammeId = programmeId,
                Description = description,
                TrainingDescription = trainingDescription,
                ShortDescription = shortDescription,
                OutcomeDescription = outcomeDescription,
                TrainingProvider = provider
            };
            var model = new VacancyPreviewViewModel();
            
            await mapper.MapFromVacancyAsync(model, vacancy);
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionOneState.Should().Be(VacancyTaskListSectionState.Completed);
            model.TaskListSectionThreeState.Should().Be(VacancyTaskListSectionState.NotStarted);
        }

        [Test, MoqAutoData]
        public async Task Then_If_There_Are_Skills_Added_Section_Set_To_Incomplete(
            string title,
            string programmeId,
            string description,
            string shortDescription,
            string trainingDescription,
            string outcomeDescription,
            List<string> skills,
            Vacancies.Client.Domain.Entities.TrainingProvider provider,
            ApprenticeshipProgramme programme,
            [Frozen] Mock<IRecruitVacancyClient> recruitVacancyClient,
            DisplayVacancyViewModelMapper mapper)
        {
            var vacancy = new Vacancy
            {
                Id = Guid.NewGuid(),
                Title = title,
                ProgrammeId = programmeId,
                Description = description,
                TrainingDescription = trainingDescription,
                ShortDescription = shortDescription,
                OutcomeDescription = outcomeDescription,
                TrainingProvider = provider,
                Skills = skills
            };
            var model = new VacancyPreviewViewModel();
            
            await mapper.MapFromVacancyAsync(model, vacancy);
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionThreeState.Should().Be(VacancyTaskListSectionState.InProgress);
        }
        
        [Test, MoqAutoData]
        public async Task Then_If_There_Are_Skills_And_Qualifications_Added_Section_Set_To_Incomplete(
            string title,
            string programmeId,
            string description,
            string shortDescription,
            string trainingDescription,
            string outcomeDescription,
            List<string> skills,
            List<Qualification> qualifications,
            Vacancies.Client.Domain.Entities.TrainingProvider provider,
            ApprenticeshipProgramme programme,
            [Frozen] Mock<IRecruitVacancyClient> recruitVacancyClient,
            DisplayVacancyViewModelMapper mapper)
        {
            var vacancy = new Vacancy
            {
                Id = Guid.NewGuid(),
                Title = title,
                ProgrammeId = programmeId,
                Description = description,
                TrainingDescription = trainingDescription,
                ShortDescription = shortDescription,
                OutcomeDescription = outcomeDescription,
                TrainingProvider = provider,
                Skills = skills,
                Qualifications = qualifications
            };
            var model = new VacancyPreviewViewModel();
            
            await mapper.MapFromVacancyAsync(model, vacancy);
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionThreeState.Should().Be(VacancyTaskListSectionState.InProgress);
        }
        
        [Test, MoqAutoData]
        public async Task Then_If_There_Are_Skills_Qualifications_And_Other_Things_To_Consider_Added_Section_Set_To_Complete(
            string title,
            string programmeId,
            string description,
            string shortDescription,
            string trainingDescription,
            string outcomeDescription,
            List<string> skills,
            string otherThingsToConsider,
            List<Qualification> qualifications,
            Vacancies.Client.Domain.Entities.TrainingProvider provider,
            ApprenticeshipProgramme programme,
            [Frozen] Mock<IRecruitVacancyClient> recruitVacancyClient,
            DisplayVacancyViewModelMapper mapper)
        {
            var vacancy = new Vacancy
            {
                Id = Guid.NewGuid(),
                Title = title,
                ProgrammeId = programmeId,
                Description = description,
                TrainingDescription = trainingDescription,
                ShortDescription = shortDescription,
                OutcomeDescription = outcomeDescription,
                TrainingProvider = provider,
                Skills = skills,
                Qualifications = qualifications,
                ThingsToConsider = otherThingsToConsider
            };
            var model = new VacancyPreviewViewModel();
            
            await mapper.MapFromVacancyAsync(model, vacancy);
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionThreeState.Should().Be(VacancyTaskListSectionState.Completed);
        }
    }
}