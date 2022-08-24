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

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.ViewModels.VacancyPreview
{
    public class WhenBuildingVacancyPreviewViewModelSectionThreeState
    {
        [Test, MoqAutoData]
        public async Task Then_The_Section_State_Is_Set_to_Not_Started(
            DisplayVacancyViewModelMapper mapper)
        {
            var vacancy = CreateCompletedSectionOneAndSectionTwoVacancy();
            vacancy.NumberOfPositions = null;
            var model = new VacancyPreviewViewModel();
            
            await mapper.MapFromVacancyAsync(model, vacancy);
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionOneState.Should().Be(VacancyTaskListSectionState.Completed);
            model.TaskListSectionTwoState.Should().Be(VacancyTaskListSectionState.InProgress);
            model.TaskListSectionThreeState.Should().Be(VacancyTaskListSectionState.NotStarted);
        }
        
        [Test, MoqAutoData]
        public async Task Then_If_There_Section_Two_Is_Completed_Then_Section_Three_Set_To_NotStarted(
            DisplayVacancyViewModelMapper mapper)
        {
            var vacancy = CreateCompletedSectionOneAndSectionTwoVacancy();
            var model = new VacancyPreviewViewModel();
            
            await mapper.MapFromVacancyAsync(model, vacancy);
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionOneState.Should().Be(VacancyTaskListSectionState.Completed);
            model.TaskListSectionTwoState.Should().Be(VacancyTaskListSectionState.Completed);
            model.TaskListSectionThreeState.Should().Be(VacancyTaskListSectionState.NotStarted);
        }

        [Test, MoqAutoData]
        public async Task Then_If_There_Are_Skills_Added_Section_Set_To_Incomplete(
            List<string> skills,
            DisplayVacancyViewModelMapper mapper)
        {
            var vacancy = CreateCompletedSectionOneAndSectionTwoVacancy();
            vacancy.Skills = skills;
            var model = new VacancyPreviewViewModel();
            
            await mapper.MapFromVacancyAsync(model, vacancy);
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionThreeState.Should().Be(VacancyTaskListSectionState.InProgress);
        }
        
        [Test, MoqAutoData]
        public async Task Then_If_There_Are_Skills_And_Qualifications_Added_Section_Set_To_Complete(
            List<string> skills,
            List<Qualification> qualifications,
            DisplayVacancyViewModelMapper mapper)
        {
            var vacancy = CreateCompletedSectionOneAndSectionTwoVacancy();
            vacancy.Skills = skills;
            vacancy.Qualifications = qualifications;
            var model = new VacancyPreviewViewModel();
            
            await mapper.MapFromVacancyAsync(model, vacancy);
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionThreeState.Should().Be(VacancyTaskListSectionState.InProgress);
        }
        
        [Test, MoqAutoData]
        public async Task Then_If_There_Are_Skills_Qualifications_And_FutureProspects_Added_Section_Set_To_Complete(
            string outcomeDescription,
            List<string> skills,
            List<Qualification> qualifications,
            DisplayVacancyViewModelMapper mapper)
        {
            var vacancy = CreateCompletedSectionOneAndSectionTwoVacancy();
            vacancy.Skills = skills;
            vacancy.Qualifications = qualifications;
            vacancy.OutcomeDescription = outcomeDescription;
            var model = new VacancyPreviewViewModel();
            
            await mapper.MapFromVacancyAsync(model, vacancy);
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionThreeState.Should().Be(VacancyTaskListSectionState.Completed);
        }
        
        [Test, MoqAutoData]
        public async Task Then_If_There_Are_Skills_Qualifications_And_Other_Things_To_Consider_Added_Section_Set_To_Complete(
            string outcomeDescription,
            List<string> skills,
            string otherThingsToConsider,
            List<Qualification> qualifications,
            DisplayVacancyViewModelMapper mapper)
        {
            var vacancy = CreateCompletedSectionOneAndSectionTwoVacancy();
            vacancy.Skills = skills;
            vacancy.Qualifications = qualifications;
            vacancy.OutcomeDescription = outcomeDescription;
            vacancy.ThingsToConsider = otherThingsToConsider;
            var model = new VacancyPreviewViewModel();
            
            await mapper.MapFromVacancyAsync(model, vacancy);
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionThreeState.Should().Be(VacancyTaskListSectionState.Completed);
        }
        
        private Vacancy CreateCompletedSectionOneAndSectionTwoVacancy()
        {
            return new Vacancy
            {
                Title = "title",
                ProgrammeId = "programmeId",
                Description = "description",
                TrainingDescription = "trainingDescription",
                ShortDescription = "shortDescription",
                TrainingProvider = new Vacancies.Client.Domain.Entities.TrainingProvider
                {
                    Address = new Address(),
                    Name = "name",
                    Ukprn = 1231231
                },
                AccountLegalEntityPublicHashedId = "accountLegalEntityPublicHashedId",
                ClosingDate = DateTime.UtcNow.AddMonths(4),
                StartDate = DateTime.UtcNow.AddMonths(5),
                Wage = new Wage
                {
                    Duration = 36,
                    DurationUnit = DurationUnit.Month,
                    WageType = WageType.NationalMinimumWage,
                    WeeklyHours = 30,
                    WorkingWeekDescription = "Monday to Thursday"
                },
                NumberOfPositions = 1,
                EmployerLocation = new Address
                {
                    AddressLine1 = "test",
                    Postcode = "test"
                }
            };
        }
    }
}