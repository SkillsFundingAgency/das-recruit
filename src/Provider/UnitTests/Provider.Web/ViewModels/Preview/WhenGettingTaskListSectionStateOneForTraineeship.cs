using System;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Provider.Web.Mappings;
using Esfa.Recruit.Provider.Web.ViewModels.VacancyPreview;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.ApprenticeshipProgrammes;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.Routes;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Moq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.ViewModels.Preview
{
    public class WhenGettingTaskListSectionStateOneForTraineeship
    {
        [Test, MoqAutoData]
        public async Task Then_The_Section_State_Is_Set(
            TrainingProvider trainingProvider,
            DisplayVacancyViewModelMapper mapper)
        {
            var vacancy = new Vacancy()
            {
                Id = Guid.NewGuid(),
                VacancyType = VacancyType.Traineeship,
                TrainingProvider = trainingProvider
            };
            var model = new VacancyPreviewViewModel();
            await mapper.MapFromVacancyAsync(model, vacancy);
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionOneState.Should().Be(VacancyTaskListSectionState.NotStarted);
        }
        
        [Test, MoqAutoData]
        public async Task Then_If_Section_Started_And_Has_Title_Then_Set_To_In_Progress(
            string accountLegalEntityPublicHashedId,
            TrainingProvider trainingProvider,
            DisplayVacancyViewModelMapper mapper)
        {
            var vacancy = new Vacancy
            {
                Id = Guid.NewGuid(),
                TrainingProvider = trainingProvider,
                AccountLegalEntityPublicHashedId = accountLegalEntityPublicHashedId,
                VacancyType = VacancyType.Traineeship
            };
            var model = new VacancyPreviewViewModel();
            await mapper.MapFromVacancyAsync(model, vacancy);
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionOneState.Should().Be(VacancyTaskListSectionState.InProgress);
        }
        
        [Test, MoqAutoData]
        public async Task Then_If_Has_Title_Training_And_Organisation_Name_Then_In_Progress(
            string title,
            string accountLegalEntityPublicHashedId,
            TrainingProvider trainingProvider,
            DisplayVacancyViewModelMapper mapper)
        {
            var vacancy = new Vacancy
            {
                Id = Guid.NewGuid(),
                TrainingProvider = trainingProvider,
                Title = title,
                AccountLegalEntityPublicHashedId = accountLegalEntityPublicHashedId,
                VacancyType = VacancyType.Traineeship
            };
            var model = new VacancyPreviewViewModel();
            await mapper.MapFromVacancyAsync(model, vacancy);
            
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionOneState.Should().Be(VacancyTaskListSectionState.InProgress);
        }
        
        [Test, MoqAutoData]
        public async Task Then_If_Has_Title_Organisation_ShortDescription_Then_In_Progress(
            string title,
            string programmeId,
            string shortDescription,
            TrainingProvider trainingProvider,
            string accountLegalEntityPublicHashedId,
            DisplayVacancyViewModelMapper mapper)
        {
            var vacancy = new Vacancy
            {
                Id = Guid.NewGuid(),
                TrainingProvider = trainingProvider,
                Title = title,
                ShortDescription = shortDescription,
                AccountLegalEntityPublicHashedId = accountLegalEntityPublicHashedId,
                VacancyType = VacancyType.Traineeship
            };
            var model = new VacancyPreviewViewModel();
            await mapper.MapFromVacancyAsync(model, vacancy);
            
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionOneState.Should().Be(VacancyTaskListSectionState.InProgress);
        }
        
        [Test, MoqAutoData]
        public async Task Then_If_Has_Title_Organisation_Sector_ShortDescription_And_Descriptions_Then_Completed(
            string title,
            string programmeId,
            string description,
            string shortDescription,
            string trainingDescription,
            string accountLegalEntityPublicHashedId,
            TrainingProvider trainingProvider,
            ApprenticeshipRoute route,
            [Frozen] Mock<IRecruitVacancyClient> recruitVacancyClient,
            DisplayVacancyViewModelMapper mapper)
        {
            recruitVacancyClient.Setup(x => x.GetApprenticeshipProgrammeAsync(It.IsAny<string>())).ReturnsAsync((ApprenticeshipProgramme)null);
            recruitVacancyClient.Setup(x => x.GetRoute(route.Id)).ReturnsAsync(route);
            var vacancy = new Vacancy
            {
                Id = Guid.NewGuid(),
                TrainingProvider = trainingProvider,
                Title = title,
                RouteId = route.Id,
                TrainingDescription = trainingDescription,
                ShortDescription = shortDescription,
                AccountLegalEntityPublicHashedId = accountLegalEntityPublicHashedId,
                VacancyType = VacancyType.Traineeship
            };
            var model = new VacancyPreviewViewModel();
            await mapper.MapFromVacancyAsync(model, vacancy);
            
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionOneState.Should().Be(VacancyTaskListSectionState.Completed);
        }
    }
}