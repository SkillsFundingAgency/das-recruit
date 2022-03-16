using System;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Provider.Web.Mappings;
using Esfa.Recruit.Provider.Web.ViewModels.VacancyPreview;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.ApprenticeshipProgrammes;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Moq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.ViewModels.Preview
{
    public class WhenGettingTaskListSectionOneState
    {
        [Test, MoqAutoData]
        public async Task Then_The_Section_State_Is_Set(
            TrainingProvider trainingProvider,
            DisplayVacancyViewModelMapper mapper)
        {
            var vacancy = new Vacancy()
            {
                Id = Guid.NewGuid(),
                TrainingProvider = trainingProvider
            };
            var model = new VacancyPreviewViewModel();
            await mapper.MapFromVacancyAsync(model, vacancy);
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionOneState.Should().Be(VacancyTaskListSectionState.NotStarted);
        }

        [Test, MoqAutoData]
        public async Task Then_If_Section_Started_Then_Set_To_In_Progress(
            string title,
            TrainingProvider trainingProvider,
            DisplayVacancyViewModelMapper mapper)
        {
            var vacancy = new Vacancy
            {
                Id = Guid.NewGuid(),
                TrainingProvider = trainingProvider,
                Title = title
            };
            var model = new VacancyPreviewViewModel();
            await mapper.MapFromVacancyAsync(model, vacancy);
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionOneState.Should().Be(VacancyTaskListSectionState.InProgress);
        }
        
        [Test, MoqAutoData]
        public async Task Then_If_Has_Title_Training_And_Provider_Then_In_Progress(
            string title,
            string programmeId,
            string employerAccountId,
            TrainingProvider trainingProvider,
            ApprenticeshipProgramme programme,
            [Frozen] Mock<IRecruitVacancyClient> recruitVacancyClient,
            DisplayVacancyViewModelMapper mapper)
        {
            recruitVacancyClient.Setup(x => x.GetApprenticeshipProgrammeAsync(programmeId)).ReturnsAsync(programme);
            var vacancy = new Vacancy
            {
                Id = Guid.NewGuid(),
                TrainingProvider = trainingProvider,
                Title = title,
                ProgrammeId = programmeId,
                EmployerAccountId = employerAccountId
            };
            var model = new VacancyPreviewViewModel();
            await mapper.MapFromVacancyAsync(model, vacancy);
            
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionOneState.Should().Be(VacancyTaskListSectionState.InProgress);
        }
        
        [Test, MoqAutoData]
        public async Task Then_If_Has_Title_Training_Provider_ShortDescription_Then_In_Progress(
            string title,
            string programmeId,
            string shortDescription,
            string employerAccountId,
            TrainingProvider trainingProvider,
            ApprenticeshipProgramme programme,
            [Frozen] Mock<IRecruitVacancyClient> recruitVacancyClient,
            DisplayVacancyViewModelMapper mapper)
        {
            recruitVacancyClient.Setup(x => x.GetApprenticeshipProgrammeAsync(programmeId)).ReturnsAsync(programme);
            var vacancy = new Vacancy
            {
                Id = Guid.NewGuid(),
                TrainingProvider = trainingProvider,
                Title = title,
                ProgrammeId = programmeId,
                ShortDescription = shortDescription,
                EmployerAccountId = employerAccountId
            };
            var model = new VacancyPreviewViewModel();
            await mapper.MapFromVacancyAsync(model, vacancy);
            
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionOneState.Should().Be(VacancyTaskListSectionState.InProgress);
        }
        
        [Test, MoqAutoData]
        public async Task Then_If_Has_Title_Training_Provider_ShortDescription_And_Descriptions_Then_Completed(
            string title,
            string programmeId,
            string description,
            string shortDescription,
            string trainingDescription,
            string outcomeDescription,
            string accountLegalEntityPublicHashedId,
            string employerAccountId,
            EmployerInfo employerInfo,
            TrainingProvider trainingProvider,
            ApprenticeshipProgramme programme,
            [Frozen] Mock<IRecruitVacancyClient> recruitVacancyClient,
            [Frozen] Mock<IProviderVacancyClient> vacancyClient,
            DisplayVacancyViewModelMapper mapper)
        {
            recruitVacancyClient.Setup(x => x.GetApprenticeshipProgrammeAsync(programmeId)).ReturnsAsync(programme);
            vacancyClient
                .Setup(x => x.GetProviderEmployerVacancyDataAsync(trainingProvider.Ukprn.Value, employerAccountId))
                .ReturnsAsync(employerInfo);
            var vacancy = new Vacancy
            {
                Id = Guid.NewGuid(),
                TrainingProvider = trainingProvider,
                Title = title,
                ProgrammeId = programmeId,
                Description = description,
                TrainingDescription = trainingDescription,
                ShortDescription = shortDescription,
                OutcomeDescription = outcomeDescription,
                EmployerAccountId = employerAccountId
            };
            var model = new VacancyPreviewViewModel();
            await mapper.MapFromVacancyAsync(model, vacancy);
            
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionOneState.Should().Be(VacancyTaskListSectionState.Completed);
        }
    }
}