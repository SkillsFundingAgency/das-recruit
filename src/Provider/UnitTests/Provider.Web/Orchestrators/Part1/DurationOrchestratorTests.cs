using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.UnitTests.Provider.Web.HardMocks;
using Esfa.Recruit.Provider.Web;
using Esfa.Recruit.Provider.Web.Orchestrators.Part1;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.Duration;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Orchestrators.Part1
{
    public class DurationOrchestratorTests
    {
        private DurationOrchestratorTestsFixture _fixture;

        public DurationOrchestratorTests()
        {
            _fixture = new DurationOrchestratorTestsFixture();
        }

        [Theory]
        [InlineData(24, DurationUnit.Month, 37.0, "this is a value", new string[] { }, new string[] { FieldIdentifiers.ExpectedDuration, FieldIdentifiers.WorkingWeek })]
        [InlineData(12, DurationUnit.Month, 37.0, "this is a value", new string[] { FieldIdentifiers.ExpectedDuration }, new string[] { FieldIdentifiers.WorkingWeek })]
        [InlineData(24, DurationUnit.Year, 37.0, "this is a value", new string[] { FieldIdentifiers.ExpectedDuration }, new string[] { FieldIdentifiers.WorkingWeek })]
        [InlineData(24, DurationUnit.Month, 35.0, "this is a value", new string[] { FieldIdentifiers.WorkingWeek }, new string[] { FieldIdentifiers.ExpectedDuration })]
        [InlineData(24, DurationUnit.Month, 37.0, "this is a new value", new string[] { FieldIdentifiers.WorkingWeek }, new string[] { FieldIdentifiers.ExpectedDuration })]
        [InlineData(1, DurationUnit.Year, 35.0, "this is a new value", new string[] { FieldIdentifiers.ExpectedDuration, FieldIdentifiers.WorkingWeek }, new string[] { })]
        public async Task WhenUpdated_ShouldFlagFieldIndicators(int duration, DurationUnit durationUnit, decimal weeklyHours, string workingWeekDescription, string[] setFieldIdentifers, string [] unsetFieldIdentifiers)
        {
            _fixture
                .WithDuration(24)
                .WithDurationUnit(DurationUnit.Month)
                .WithWeeklyHours(37)
                .WithWorkingWeekDescription("this is a value")
                .Setup();

            var durationEditModel = new DurationEditModel
            {
                Ukprn = _fixture.Vacancy.TrainingProvider.Ukprn.Value,
                VacancyId = _fixture.Vacancy.Id,
                Duration = duration.ToString(),
                DurationUnit = durationUnit,
                WeeklyHours = weeklyHours.ToString(),
                WorkingWeekDescription = workingWeekDescription
            };

            await _fixture.PostDurationEditModelAsync(durationEditModel);

            _fixture.VerifyEmployerReviewFieldIndicators(setFieldIdentifers, unsetFieldIdentifiers);
        }

        public class DurationOrchestratorTestsFixture
        {
            private const VacancyRuleSet ValidationRules = VacancyRuleSet.Duration | VacancyRuleSet.WorkingWeekDescription | VacancyRuleSet.WeeklyHours;
            public VacancyUser User { get; }
            public Vacancy Vacancy { get; }
            public DurationOrchestrator Sut {get; private set;}

            public DurationOrchestratorTestsFixture()
            {
                MockClient = new Mock<IProviderVacancyClient>();
                MockRecruitVacancyClient = new Mock<IRecruitVacancyClient>();

                User = VacancyOrchestratorTestData.GetVacancyUser();
                Vacancy = VacancyOrchestratorTestData.GetPart1CompleteVacancy();
            }

            public DurationOrchestratorTestsFixture WithDuration(int duration)
            {
                Vacancy.Wage.Duration = duration;
                return this;
            }

            public DurationOrchestratorTestsFixture WithDurationUnit(DurationUnit durationUnit)
            {
                Vacancy.Wage.DurationUnit = durationUnit;
                return this;
            }

            public DurationOrchestratorTestsFixture WithWeeklyHours(decimal weeklyHours)
            {
                Vacancy.Wage.WeeklyHours = weeklyHours;
                return this;
            }

            public DurationOrchestratorTestsFixture WithWorkingWeekDescription(string workingWeekDescription)
            {
                Vacancy.Wage.WorkingWeekDescription = workingWeekDescription;
                return this;
            }

            public void Setup()
            {
                MockRecruitVacancyClient.Setup(x => x.GetVacancyAsync(Vacancy.Id)).ReturnsAsync(Vacancy);
                MockRecruitVacancyClient.Setup(x => x.Validate(Vacancy, ValidationRules)).Returns(new EntityValidationResult());
                MockRecruitVacancyClient.Setup(x => x.UpdateDraftVacancyAsync(It.IsAny<Vacancy>(), User));
                MockRecruitVacancyClient.Setup(x => x.UpdateEmployerProfileAsync(It.IsAny<EmployerProfile>(), User));

                Sut = new DurationOrchestrator(MockRecruitVacancyClient.Object, Mock.Of<ILogger<DurationOrchestrator>>(), 
                    Mock.Of<IReviewSummaryService>(), new Utility(MockRecruitVacancyClient.Object));
            }

            public async Task PostDurationEditModelAsync(DurationEditModel model)
            {
                await Sut.PostDurationEditModelAsync(model, User);
            }

            public void VerifyEmployerReviewFieldIndicators(string[] setFieldIdentifiers, string[] unsetFieldIdentifiers)
            {
                foreach (var fieldIdentifier in setFieldIdentifiers)
                {
                    VerifyProviderReviewFieldIndicators(fieldIdentifier, true);
                }

                foreach (var fieldIdentifier in unsetFieldIdentifiers)
                {
                    VerifyProviderReviewFieldIndicators(fieldIdentifier, false);
                }
            }

            public void VerifyProviderReviewFieldIndicators(string fieldIdentifier, bool value)
            {
                Vacancy.ProviderReviewFieldIndicators
                    .Where(p => p.FieldIdentifier == fieldIdentifier).Single()
                    .Should().NotBeNull().And
                    .Match<ProviderReviewFieldIndicator>((x) => x.IsChangeRequested == value);
            }

            public Mock<IProviderVacancyClient> MockClient { get; set; }
            public Mock<IRecruitVacancyClient> MockRecruitVacancyClient { get; set; }
        }
    }
}
