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
        public async Task And_No_Values_Then_Not_Started(
            DisplayVacancyViewModelMapper mapper)
        {
            var vacancy = new Vacancy
            {
                Id = Guid.NewGuid()
            };
            var model = new VacancyPreviewViewModel();
            await mapper.MapFromVacancyAsync(model, vacancy);
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionTwoState.Should().Be(VacancyTaskListSectionState.NotStarted);
        }

        [Test, MoqAutoData]
        public async Task And_Has_Wage_Then_In_Progress(
            Wage wage,
            DateTime startDate,
            DisplayVacancyViewModelMapper mapper)
        {
            var vacancy = new Vacancy
            {
                Id = Guid.NewGuid(),
                Wage = wage,
                StartDate = startDate
            };
            var model = new VacancyPreviewViewModel();
            await mapper.MapFromVacancyAsync(model, vacancy);
            
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionTwoState.Should().Be(VacancyTaskListSectionState.InProgress);
        }

        [Test, MoqAutoData]
        public async Task And_Has_Duration_Then_In_Progress(
            Wage wage,
            DateTime startDate,
            DisplayVacancyViewModelMapper mapper)
        {
            var vacancy = new Vacancy
            {
                Id = Guid.NewGuid(),
                Wage = wage,
                StartDate = startDate
            };
            var model = new VacancyPreviewViewModel();
            await mapper.MapFromVacancyAsync(model, vacancy);
            
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionTwoState.Should().Be(VacancyTaskListSectionState.InProgress);
        }
        
        [Test, MoqAutoData]
        public async Task And_Has_Important_Dates_Then_In_Progress(
            Wage wage,
            DateTime closingDate,
            DisplayVacancyViewModelMapper mapper)
        {
            var vacancy = new Vacancy
            {
                Id = Guid.NewGuid(),
                Wage = wage,
                ClosingDate = closingDate,
                StartDate = closingDate.AddMonths(1)
            };
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
            var vacancy = new Vacancy
            {
                Id = Guid.NewGuid(),
                Wage = wage,
                ClosingDate = closingDate,
                StartDate = closingDate.AddMonths(1),
                NumberOfPositions = numberOfPositions
            };
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
            var vacancy = new Vacancy
            {
                Id = Guid.NewGuid(),
                Wage = wage,
                ClosingDate = closingDate,
                StartDate = closingDate.AddMonths(1),
                EmployerLocation = employerLocation 
            };
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
            var vacancy = new Vacancy
            {
                Id = Guid.NewGuid(),
                Wage = wage,
                ClosingDate = closingDate,
                StartDate = closingDate.AddMonths(1),
                NumberOfPositions = numberOfPositions,
                EmployerLocation = employerLocation
            };
            var model = new VacancyPreviewViewModel();
            await mapper.MapFromVacancyAsync(model, vacancy);
            
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionTwoState.Should().Be(VacancyTaskListSectionState.Completed);
        }
    }
}