using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.UnitTests.Provider.Web.HardMocks;
using Esfa.Recruit.Provider.Web.Orchestrators.Part2;
using Esfa.Recruit.Provider.Web.ViewModels.Part2.Considerations;
using Esfa.Recruit.Provider.Web.ViewModels.Part2.FutureProspects;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;


namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Orchestrators.Part2
{
    public class FutureProspectsOrchestratorTests
    {
        private FutureProspectsOrchestratorTestsFixture _fixture;

        public FutureProspectsOrchestratorTests()
        {
            _fixture = new FutureProspectsOrchestratorTestsFixture();
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

            await _fixture.PostFutureProspectsEditModelAsync(futureProspectsEditModel);

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
                FutureProspects = "has a new value"
            };

            await _fixture.PostFutureProspectsEditModelAsync(futureProspectsEditModel);

            _fixture.VerifyProviderReviewFieldIndicators(FieldIdentifiers.OutcomeDescription, true);
        }

        public class FutureProspectsOrchestratorTestsFixture
        {
            private const VacancyRuleSet ValidationRules = VacancyRuleSet.OutcomeDescription;
            public VacancyUser User { get; }
            public Vacancy Vacancy { get; }
            public FutureProspectsOrchestrator Sut { get; private set; }

            public FutureProspectsOrchestratorTestsFixture()
            {
                MockClient = new Mock<IProviderVacancyClient>();
                MockRecruitVacancyClient = new Mock<IRecruitVacancyClient>();

                User = VacancyOrchestratorTestData.GetVacancyUser();
                Vacancy = VacancyOrchestratorTestData.GetPart1CompleteVacancy();
            }

            public FutureProspectsOrchestratorTestsFixture WithFutureProspects(string futureProspects)
            {
                Vacancy.OutcomeDescription = futureProspects;
                return this;
            }

            public void Setup()
            {
                MockRecruitVacancyClient.Setup(x => x.GetVacancyAsync(Vacancy.Id)).ReturnsAsync(Vacancy);
                MockRecruitVacancyClient.Setup(x => x.Validate(Vacancy, ValidationRules)).Returns(new EntityValidationResult());
                MockRecruitVacancyClient.Setup(x => x.UpdateDraftVacancyAsync(It.IsAny<Vacancy>(), User));
                MockRecruitVacancyClient.Setup(x => x.UpdateEmployerProfileAsync(It.IsAny<EmployerProfile>(), User));

                Sut = new FutureProspectsOrchestrator(MockClient.Object, MockRecruitVacancyClient.Object,
                    Mock.Of<ILogger<FutureProspectsOrchestrator>>(), Mock.Of<IReviewSummaryService>());
            }

            public async Task PostFutureProspectsEditModelAsync(FutureProspectsEditModel model)
            {
                await Sut.PostFutureProspectsEditModelAsync(model, User);
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
