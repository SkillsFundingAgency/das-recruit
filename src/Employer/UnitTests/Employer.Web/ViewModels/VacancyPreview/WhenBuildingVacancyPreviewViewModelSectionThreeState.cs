using System.Collections.Generic;
using AutoFixture.NUnit3;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.ViewModels.VacancyPreview;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.ViewModels.VacancyPreview
{
    public class WhenBuildingVacancyPreviewViewModelSectionThreeState
    {
        [Test, MoqAutoData]
        public async Task Then_The_Section_State_Is_Set_to_Not_Started(
            ApprenticeshipStandard standard,
            [Frozen] Mock<IApprenticeshipProgrammeProvider> apprenticeshipProgrammeProvider,
            DisplayVacancyViewModelMapper mapper)
        {
            
            var vacancy = CreateCompletedSectionOneAndSectionTwoVacancy(apprenticeshipProgrammeProvider, standard);
            
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
            ApprenticeshipStandard standard,
            [Frozen] Mock<IApprenticeshipProgrammeProvider> apprenticeshipProgrammeProvider,
            DisplayVacancyViewModelMapper mapper)
        {
            var vacancy = CreateCompletedSectionOneAndSectionTwoVacancy(apprenticeshipProgrammeProvider, standard);
            var model = new VacancyPreviewViewModel();
            
            await mapper.MapFromVacancyAsync(model, vacancy);
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionOneState.Should().Be(VacancyTaskListSectionState.Completed);
            model.TaskListSectionTwoState.Should().Be(VacancyTaskListSectionState.Completed);
            model.TaskListSectionThreeState.Should().Be(VacancyTaskListSectionState.NotStarted);
        }

        [Test, MoqAutoData]
        public async Task Then_If_There_Are_Skills_Added_Section_Set_To_Incomplete(
            ApprenticeshipStandard standard,
            [Frozen] Mock<IApprenticeshipProgrammeProvider> apprenticeshipProgrammeProvider,
            List<string> skills,
            DisplayVacancyViewModelMapper mapper)
        {
            var vacancy = CreateCompletedSectionOneAndSectionTwoVacancy(apprenticeshipProgrammeProvider, standard);
            vacancy.Skills = skills;
            var model = new VacancyPreviewViewModel();
            
            await mapper.MapFromVacancyAsync(model, vacancy);
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionThreeState.Should().Be(VacancyTaskListSectionState.InProgress);
        }
        
        [Test, MoqAutoData]
        public async Task Then_If_There_Are_Skills_And_Qualifications_Added_Section_Set_To_Complete(
            ApprenticeshipStandard standard,
            [Frozen] Mock<IApprenticeshipProgrammeProvider> apprenticeshipProgrammeProvider,
            List<string> skills,
            List<Qualification> qualifications,
            DisplayVacancyViewModelMapper mapper)
        {
            var vacancy = CreateCompletedSectionOneAndSectionTwoVacancy(apprenticeshipProgrammeProvider, standard);
            vacancy.Skills = skills;
            vacancy.Qualifications = qualifications;
            vacancy.HasOptedToAddQualifications = true;
            var model = new VacancyPreviewViewModel();
            
            await mapper.MapFromVacancyAsync(model, vacancy);
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionThreeState.Should().Be(VacancyTaskListSectionState.InProgress);
        }
        
        [Test, MoqAutoData]
        public async Task Then_If_There_Are_Skills_Qualifications_And_FutureProspects_Added_Section_Set_To_Complete(
            ApprenticeshipStandard standard,
            [Frozen] Mock<IApprenticeshipProgrammeProvider> apprenticeshipProgrammeProvider,
            string outcomeDescription,
            List<string> skills,
            List<Qualification> qualifications,
            DisplayVacancyViewModelMapper mapper)
        {
            var vacancy = CreateCompletedSectionOneAndSectionTwoVacancy(apprenticeshipProgrammeProvider, standard);
            vacancy.Skills = skills;
            vacancy.Qualifications = qualifications;
            vacancy.OutcomeDescription = outcomeDescription;
            vacancy.HasOptedToAddQualifications = true;
            var model = new VacancyPreviewViewModel();
            
            await mapper.MapFromVacancyAsync(model, vacancy);
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionThreeState.Should().Be(VacancyTaskListSectionState.Completed);
        }
        
        [Test, MoqAutoData]
        public async Task Then_If_There_Are_Skills_And_Opted_Not_To_Add_Qualifications_And_FutureProspects_Added_Section_Set_To_Complete(
            string outcomeDescription,
            List<string> skills,
            List<Qualification> qualifications,
            ApprenticeshipStandard standard,
            [Frozen] Mock<IApprenticeshipProgrammeProvider> apprenticeshipProgrammeProvider,
            [Frozen] Mock<IFeature> feature,
            DisplayVacancyViewModelMapper mapper)
        {
            var vacancy = CreateCompletedSectionOneAndSectionTwoVacancy(apprenticeshipProgrammeProvider, standard);
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
        public async Task Then_If_There_Are_Skills_Qualifications_And_Other_Things_To_Consider_Added_Section_Set_To_Complete(
            string outcomeDescription,
            List<string> skills,
            string otherThingsToConsider,
            List<Qualification> qualifications,
            ApprenticeshipStandard standard,
            [Frozen] Mock<IApprenticeshipProgrammeProvider> apprenticeshipProgrammeProvider,
            DisplayVacancyViewModelMapper mapper)
        {
            var vacancy = CreateCompletedSectionOneAndSectionTwoVacancy(apprenticeshipProgrammeProvider, standard);
            vacancy.Skills = skills;
            vacancy.Qualifications = qualifications;
            vacancy.HasOptedToAddQualifications = true;
            vacancy.OutcomeDescription = outcomeDescription;
            vacancy.ThingsToConsider = otherThingsToConsider;
            var model = new VacancyPreviewViewModel();
            
            await mapper.MapFromVacancyAsync(model, vacancy);
            model.SetSectionStates(model, new ModelStateDictionary());

            model.TaskListSectionThreeState.Should().Be(VacancyTaskListSectionState.Completed);
        }
        
        private Vacancy CreateCompletedSectionOneAndSectionTwoVacancy(Mock<IApprenticeshipProgrammeProvider> apprenticeshipProgrammeProvider, ApprenticeshipStandard standard)
        {
            var standardId = 10;
            apprenticeshipProgrammeProvider.Setup(x => x.GetApprenticeshipStandardVacancyPreviewData(standardId))
                .ReturnsAsync(standard);
            return new Vacancy
            {
                Id = new Guid(),
                EmployerAccountId = "employerAccountId",
                Title = "title",
                ProgrammeId = standardId.ToString(),
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
                },
                Qualifications = []
            };
        }
    }
}