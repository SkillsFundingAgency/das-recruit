using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.ViewModels.VacancyPreview;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.ViewModels.VacancyPreview
{
    public class WhenGettingTaskListSectionTwoState
    {
        [Test, MoqAutoData]
        public async Task And_Section_One_Not_Complete_Then_Not_Started(
            DisplayVacancyViewModelMapper mapper)
        {
            var vacancy = CreateCompletedSectionOneVacancy();
            vacancy.OutcomeDescription = null;
            vacancy.Id = Guid.NewGuid();
            var model = new VacancyPreviewViewModel();
            await mapper.MapFromVacancyAsync(model, vacancy);
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionTwoState.Should().Be(VacancyTaskListSectionState.NotStarted);
        }

        [Test, MoqAutoData]
        public async Task And_Section_One_Completed_And_Important_Dates_Not_Entered_Then_Not_Started(DisplayVacancyViewModelMapper mapper)
        {
            var vacancy = CreateCompletedSectionOneVacancy();
            vacancy.Id = Guid.NewGuid();
            var model = new VacancyPreviewViewModel();
            await mapper.MapFromVacancyAsync(model, vacancy);
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionTwoState.Should().Be(VacancyTaskListSectionState.NotStarted);
        }
        
        [Test, MoqAutoData]
        public async Task And_Has_Important_Dates_Then_In_Progress(
            DateTime closingDate,
            DisplayVacancyViewModelMapper mapper)
        {
            var vacancy = CreateCompletedSectionOneVacancy();
            vacancy.Id = Guid.NewGuid();
            vacancy.ClosingDate = closingDate;
            vacancy.StartDate = closingDate.AddMonths(1);
            var model = new VacancyPreviewViewModel();
            await mapper.MapFromVacancyAsync(model, vacancy);
            
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionTwoState.Should().Be(VacancyTaskListSectionState.InProgress);
        }

        [Test, MoqAutoData]
        public async Task And_Has_Wage_Then_In_Progress(
            DateTime closingDate,
            Wage wage,
            DisplayVacancyViewModelMapper mapper)
        {
            var vacancy = CreateCompletedSectionOneVacancy();
            vacancy.Id = Guid.NewGuid();
            vacancy.Wage = wage;
            vacancy.ClosingDate = closingDate;
            vacancy.StartDate = closingDate.AddMonths(1);
            var model = new VacancyPreviewViewModel();
            await mapper.MapFromVacancyAsync(model, vacancy);
            
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionTwoState.Should().Be(VacancyTaskListSectionState.InProgress);
        }

        [Test, MoqAutoData]
        public async Task And_Has_Duration_Then_In_Progress(
            Wage wage,
            DateTime closingDate,
            DisplayVacancyViewModelMapper mapper)
        {
            var vacancy = CreateCompletedSectionOneVacancy();
            vacancy.Id = Guid.NewGuid();
            vacancy.Wage = wage;
            vacancy.ClosingDate = closingDate;
            vacancy.StartDate = closingDate.AddMonths(1);
            var model = new VacancyPreviewViewModel();
            await mapper.MapFromVacancyAsync(model, vacancy);
            
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionTwoState.Should().Be(VacancyTaskListSectionState.InProgress);
        }

        [Test, MoqAutoData]
        public async Task And_Has_Number_Of_Positions_Then_In_Progress(
            Wage wage,
            DateTime closingDate,
            int numberOfPositions,
            DisplayVacancyViewModelMapper mapper)
        {
            var vacancy = CreateCompletedSectionOneVacancy();
            vacancy.Id = Guid.NewGuid();
            vacancy.Wage = wage;
            vacancy.ClosingDate = closingDate;
            vacancy.StartDate = closingDate.AddMonths(1);
            vacancy.NumberOfPositions = numberOfPositions;
            var model = new VacancyPreviewViewModel();
            await mapper.MapFromVacancyAsync(model, vacancy);
            
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionTwoState.Should().Be(VacancyTaskListSectionState.InProgress);
        }
        
        [Test, MoqAutoData]
        public async Task And_Has_Location_Then_In_Progress(
            Wage wage,
            DateTime closingDate,
            Address employerLocation,
            DisplayVacancyViewModelMapper mapper)
        {
            var vacancy = CreateCompletedSectionOneVacancy();
            vacancy.Id = Guid.NewGuid();
            vacancy.Wage = wage;
            vacancy.ClosingDate = closingDate;
            vacancy.StartDate = closingDate.AddMonths(1);
            vacancy.EmployerLocation = employerLocation;
            var model = new VacancyPreviewViewModel();
            await mapper.MapFromVacancyAsync(model, vacancy);
            
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionTwoState.Should().Be(VacancyTaskListSectionState.InProgress);
        }
        
        [Test, MoqAutoData]
        public async Task And_Has_All_Values_Then_Completed(
            Wage wage,
            DateTime closingDate,
            int numberOfPositions,
            Address employerLocation,
            DisplayVacancyViewModelMapper mapper)
        {
            var vacancy = CreateCompletedSectionOneVacancy();
            vacancy.Id = Guid.NewGuid();
            vacancy.Wage = wage;
            vacancy.ClosingDate = closingDate;
            vacancy.StartDate = closingDate.AddMonths(1);
            vacancy.NumberOfPositions = numberOfPositions;
            vacancy.EmployerLocation = employerLocation;
        
            var model = new VacancyPreviewViewModel();
            await mapper.MapFromVacancyAsync(model, vacancy);
            
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionTwoState.Should().Be(VacancyTaskListSectionState.Completed);
        }

        private Vacancy CreateCompletedSectionOneVacancy()
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
                AccountLegalEntityPublicHashedId = "accountLegalEntityPublicHashedId"
            };
        }
    }
}