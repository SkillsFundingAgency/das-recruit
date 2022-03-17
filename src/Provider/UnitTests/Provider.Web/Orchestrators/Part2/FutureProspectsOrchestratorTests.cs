using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.ViewModels.Part2.FutureProspects;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;


namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Orchestrators.Part2
{
    class FutureProspectsOrchestratorTests
    {
        private FutureProspectsOrchestratorTestsFixture _fixture;

        public FutureProspectsOrchestratorTests()
        {
            _fixture = new ConsiderationsOrchestratorTestsFixture();
        }

        [Fact]
        public async Task WhenUpdated__ShouldCallUpdateDraftVacancy()
        {
            _fixture
                .WithFutureProspects("has a value")
                .Setup();

            var futureProspectsEditModel = new FutureProspectsEditModel
            {
                Ukprn = _fixture.Vacancy.TrainingProvider.Ukprn.Value,
                VacancyId = _fixture.Vacancy.Id,
                FutureProspects = "has a new value"
            };

            await _fixture.PostConsiderationsEditModelAsync(futureProspectsEditModel);

            _fixture.VerifyUpdateDraftVacancyAsyncIsCalled();
        }

        [Fact]
        public async Task WhenFutureProspectsIsUpdated_ShouldFlagFutureProspectsFieldIndicator()
        {
            _fixture
                .WithFutureProspects("has a value")
                .Setup();

            var futureProspectsEditModel = new FutureProspectsEditModel
            {
                Ukprn = _fixture.Vacancy.TrainingProvider.Ukprn.Value,
                VacancyId = _fixture.Vacancy.Id,
                ThingsToConsider = "has a new value"
            };

            await _fixture.PostConsiderationsEditModelAsync(thingsToConsiderEditModel);

            _fixture.VerifyProviderReviewFieldIndicators(FieldIdentifiers.ThingsToConsider, true);
        }

        public class ConsiderationsOrchestratorTestsFixture
        {
            private const VacancyRuleSet ValidationRules = VacancyRuleSet.ThingsToConsider;
            public VacancyUser User { get; }
            public Vacancy Vacancy { get; }
            public ConsiderationsOrchestrator Sut { get; private set; }

            public ConsiderationsOrchestratorTestsFixture()
            {
                MockClient = new Mock<IProviderVacancyClient>();
                MockRecruitVacancyClient = new Mock<IRecruitVacancyClient>();

                User = VacancyOrchestratorTestData.GetVacancyUser();
                Vacancy = VacancyOrchestratorTestData.GetPart1CompleteVacancy();
            }

            public ConsiderationsOrchestratorTestsFixture WithThingsToConsider(string thingsToConsider)
            {
                Vacancy.ThingsToConsider = thingsToConsider;
                return this;
            }

            public void Setup()
            {
                MockRecruitVacancyClient.Setup(x => x.GetVacancyAsync(Vacancy.Id)).ReturnsAsync(Vacancy);
                MockRecruitVacancyClient.Setup(x => x.Validate(Vacancy, ValidationRules)).Returns(new EntityValidationResult());
                MockRecruitVacancyClient.Setup(x => x.UpdateDraftVacancyAsync(It.IsAny<Vacancy>(), User));
                MockRecruitVacancyClient.Setup(x => x.UpdateEmployerProfileAsync(It.IsAny<EmployerProfile>(), User));

                Sut = new ConsiderationsOrchestrator(Mock.Of<ILogger<ConsiderationsOrchestrator>>(), MockClient.Object, MockRecruitVacancyClient.Object, Mock.Of<IReviewSummaryService>());
            }

            public async Task PostConsiderationsEditModelAsync(ConsiderationsEditModel model)
            {
                await Sut.PostConsiderationsEditModelAsync(model, User);
            }

            public void VerifyProviderReviewFieldIndicators(string fieldIdentifier, bool value)
            {
                Vacancy.ProviderReviewFieldIndicators
                    .Where(p => p.FieldIdentifier == fieldIdentifier).FirstOrDefault()
                    .Should().NotBeNull().And
                    .Match<ProviderReviewFieldIndicator>((x) => x.IsChangeRequested == value);
            }

            public void VerifyUpdateDraftVacancyAsyncIsCalled()
            {
                MockRecruitVacancyClient.Verify(x => x.UpdateDraftVacancyAsync(Vacancy, User), Times.Once);
            }

            public Mock<IProviderVacancyClient> MockClient { get; set; }
            public Mock<IRecruitVacancyClient> MockRecruitVacancyClient { get; set; }
        }
    }
}
