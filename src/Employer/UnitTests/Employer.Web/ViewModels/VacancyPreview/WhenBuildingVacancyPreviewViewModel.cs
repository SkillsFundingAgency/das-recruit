using System;
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
    public class WhenBuildingVacancyPreviewViewModel
    {
        [Test, MoqAutoData]
        public async Task Then_The_Section_State_Is_Set(
            DisplayVacancyViewModelMapper mapper)
        {
            var vacancy = new Vacancy()
            {
                Id = Guid.NewGuid()
            };
            var model = new VacancyPreviewViewModel();
            await mapper.MapFromVacancyAsync(model, vacancy);
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionOneState.Should().Be(VacancyTaskListSectionState.NotStarted);
        }

        [Test, MoqAutoData]
        public async Task Then_If_Section_Started_Then_Set_To_In_Progress(
            string title,
            DisplayVacancyViewModelMapper mapper)
        {
            var vacancy = new Vacancy()
            {
                Id = Guid.NewGuid(),
                Title = title
            };
            var model = new VacancyPreviewViewModel();
            await mapper.MapFromVacancyAsync(model, vacancy);
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionOneState.Should().Be(VacancyTaskListSectionState.InProgress);
        }

        [Test, MoqAutoData]
        public async Task Then_If_Has_Title_And_Training_Then_In_Progress(
            string title,
            string programmeId,
            ApprenticeshipProgramme programme,
            [Frozen] Mock<IRecruitVacancyClient> recruitVacancyClient,
            DisplayVacancyViewModelMapper mapper)
        {
            recruitVacancyClient.Setup(x => x.GetApprenticeshipProgrammeAsync(programmeId)).ReturnsAsync(programme);
            var vacancy = new Vacancy
            {
                Id = Guid.NewGuid(),
                Title = title,
                ProgrammeId = programmeId
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
            Vacancies.Client.Domain.Entities.TrainingProvider provider,
            ApprenticeshipProgramme programme,
            [Frozen] Mock<IRecruitVacancyClient> recruitVacancyClient,
            DisplayVacancyViewModelMapper mapper)
        {
            recruitVacancyClient.Setup(x => x.GetApprenticeshipProgrammeAsync(programmeId)).ReturnsAsync(programme);
            var vacancy = new Vacancy
            {
                Id = Guid.NewGuid(),
                Title = title,
                ProgrammeId = programmeId,
                TrainingProvider = provider
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
            Vacancies.Client.Domain.Entities.TrainingProvider provider,
            ApprenticeshipProgramme programme,
            [Frozen] Mock<IRecruitVacancyClient> recruitVacancyClient,
            DisplayVacancyViewModelMapper mapper)
        {
            recruitVacancyClient.Setup(x => x.GetApprenticeshipProgrammeAsync(programmeId)).ReturnsAsync(programme);
            var vacancy = new Vacancy
            {
                Id = Guid.NewGuid(),
                Title = title,
                ProgrammeId = programmeId,
                ShortDescription = shortDescription,
                TrainingProvider = provider
            };
            var model = new VacancyPreviewViewModel();
            await mapper.MapFromVacancyAsync(model, vacancy);
            
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionOneState.Should().Be(VacancyTaskListSectionState.InProgress);
        }
        
        [Test, MoqAutoData]
        public async Task Then_If_Has_Title_Training_Provider_ShortDescription_Ale_Then_In_Progress(
            string title,
            string programmeId,
            string shortDescription,
            string accountLegalEntityPublicHashedId,
            Vacancies.Client.Domain.Entities.TrainingProvider provider,
            ApprenticeshipProgramme programme,
            [Frozen] Mock<IRecruitVacancyClient> recruitVacancyClient,
            DisplayVacancyViewModelMapper mapper)
        {
            recruitVacancyClient.Setup(x => x.GetApprenticeshipProgrammeAsync(programmeId)).ReturnsAsync(programme);
            var vacancy = new Vacancy
            {
                Id = Guid.NewGuid(),
                Title = title,
                ProgrammeId = programmeId,
                ShortDescription = shortDescription,
                TrainingProvider = provider,
                AccountLegalEntityPublicHashedId = accountLegalEntityPublicHashedId
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
            string accountLegalEntityPublicHashedId,
            Vacancies.Client.Domain.Entities.TrainingProvider provider,
            ApprenticeshipProgramme programme,
            [Frozen] Mock<IRecruitVacancyClient> recruitVacancyClient,
            DisplayVacancyViewModelMapper mapper)
        {
            recruitVacancyClient.Setup(x => x.GetApprenticeshipProgrammeAsync(programmeId)).ReturnsAsync(programme);
            var vacancy = new Vacancy
            {
                Id = Guid.NewGuid(),
                Title = title,
                ProgrammeId = programmeId,
                Description = description,
                TrainingDescription = trainingDescription,
                ShortDescription = shortDescription,
                TrainingProvider = provider,
                AccountLegalEntityPublicHashedId = accountLegalEntityPublicHashedId 
            };
            var model = new VacancyPreviewViewModel();
            await mapper.MapFromVacancyAsync(model, vacancy);
            
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionOneState.Should().Be(VacancyTaskListSectionState.Completed);
        }
    }
}