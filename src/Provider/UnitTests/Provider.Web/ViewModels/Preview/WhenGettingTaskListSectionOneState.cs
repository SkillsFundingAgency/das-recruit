using Esfa.Recruit.Provider.Web.Mappings;
using Esfa.Recruit.Provider.Web.ViewModels.VacancyPreview;
using Esfa.Recruit.Shared.Web.ViewModels.TaskList;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.ApprenticeshipProgrammes;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using NUnit.Framework;

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
        public async Task Then_If_Section_Started_And_Has_LegalEntity_Then_Set_To_In_Progress(
            string accountLegalEntityPublicHashedId,
            TrainingProvider trainingProvider,
            DisplayVacancyViewModelMapper mapper)
        {
            var vacancy = new Vacancy
            {
                Id = Guid.NewGuid(),
                TrainingProvider = trainingProvider,
                AccountLegalEntityPublicHashedId = accountLegalEntityPublicHashedId
            };
            var model = new VacancyPreviewViewModel();
            await mapper.MapFromVacancyAsync(model, vacancy);
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionOneState.Should().Be(VacancyTaskListSectionState.InProgress);
        }
        [Test, MoqAutoData]
        public async Task Then_If_Has_Title_And_AccountLegalEntityPublicHashedId_Then_In_Progress(
            string title,
            string accountLegalEntityPublicHashedId,
            TrainingProvider trainingProvider,
            ApprenticeshipProgramme programme,
            DisplayVacancyViewModelMapper mapper)
        {
            var vacancy = new Vacancy
            {
                Id = Guid.NewGuid(),
                TrainingProvider = trainingProvider,
                Title = title,
                AccountLegalEntityPublicHashedId = accountLegalEntityPublicHashedId
            };
            var model = new VacancyPreviewViewModel();
            await mapper.MapFromVacancyAsync(model, vacancy);
            
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionOneState.Should().Be(VacancyTaskListSectionState.InProgress);
        }
        
        [Test, MoqAutoData]
        public async Task Then_If_Has_Title_Training_And_AccountLegalEntityPublicHashedId_Then_In_Progress(
            string title,
            string programmeId,
            string accountLegalEntityPublicHashedId,
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
                AccountLegalEntityPublicHashedId = accountLegalEntityPublicHashedId,
                ProgrammeId = programmeId
            };
            var model = new VacancyPreviewViewModel();
            await mapper.MapFromVacancyAsync(model, vacancy);
            
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionOneState.Should().Be(VacancyTaskListSectionState.InProgress);
        }
        
        [Test, MoqAutoData]
        public async Task Then_If_Has_Title_Course_ShortDescription_Then_In_Progress(
            string title,
            int programmeId,
            string shortDescription,
            string accountLegalEntityPublicHashedId,
            TrainingProvider trainingProvider,
            ApprenticeshipStandard standard,
            [Frozen] Mock<IApprenticeshipProgrammeProvider> apprenticeshipProgrammeProvider,
            DisplayVacancyViewModelMapper mapper)
        {
            var standardId = 10;
            apprenticeshipProgrammeProvider.Setup(x => x.GetApprenticeshipStandardVacancyPreviewData(standardId))
                .ReturnsAsync(standard);
            var vacancy = new Vacancy
            {
                Id = Guid.NewGuid(),
                TrainingProvider = trainingProvider,
                Title = title,
                ProgrammeId = programmeId.ToString(),
                ShortDescription = shortDescription,
                AccountLegalEntityPublicHashedId = accountLegalEntityPublicHashedId,
            };
            var model = new VacancyPreviewViewModel();
            await mapper.MapFromVacancyAsync(model, vacancy);
            
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionOneState.Should().Be(VacancyTaskListSectionState.InProgress);
        }
        
        [Test, MoqAutoData]
        public async Task Then_If_Has_Title_Organisation_Course_ShortDescription_And_Descriptions_Then_Completed(
            string title,
            int programmeId,
            string description,
            string shortDescription,
            string trainingDescription,
            string accountLegalEntityPublicHashedId,
            TrainingProvider trainingProvider,
            ApprenticeshipStandard standard,
            [Frozen] Mock<IApprenticeshipProgrammeProvider> apprenticeshipProgrammeProvider,
            DisplayVacancyViewModelMapper mapper)
        {
            var standardId = 10;
            apprenticeshipProgrammeProvider.Setup(x => x.GetApprenticeshipStandardVacancyPreviewData(standardId))
                .ReturnsAsync(standard);
            
            var vacancy = new Vacancy
            {
                Id = Guid.NewGuid(),
                TrainingProvider = trainingProvider,
                Title = title,
                ProgrammeId = programmeId.ToString(),
                Description = description,
                TrainingDescription = trainingDescription,
                ShortDescription = shortDescription,
                AccountLegalEntityPublicHashedId = accountLegalEntityPublicHashedId
            };
            var model = new VacancyPreviewViewModel();
            await mapper.MapFromVacancyAsync(model, vacancy);
            
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionOneState.Should().Be(VacancyTaskListSectionState.Completed);
        }
        [Test, MoqAutoData]
        public async Task Then_If_Has_Title_Training_Provider_ShortDescription_And_Description_Then_Completed_For_Faa_V2(
            string employerAccountId,
            string title,
            int programmeId,
            string description,
            string shortDescription,
            string trainingDescription,
            string accountLegalEntityPublicHashedId,
            Vacancies.Client.Domain.Entities.TrainingProvider provider,
            ApprenticeshipStandard standard,
            [Frozen] Mock<IApprenticeshipProgrammeProvider> apprenticeshipProgrammeProvider,
            DisplayVacancyViewModelMapper mapper)
        {
            apprenticeshipProgrammeProvider.Setup(x => x.GetApprenticeshipStandardVacancyPreviewData(programmeId))
                .ReturnsAsync(standard);
            
            var vacancy = new Vacancy
            {
                Id = Guid.NewGuid(),
                EmployerAccountId = employerAccountId,
                Title = title,
                ProgrammeId = programmeId.ToString(),
                Description = description,
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