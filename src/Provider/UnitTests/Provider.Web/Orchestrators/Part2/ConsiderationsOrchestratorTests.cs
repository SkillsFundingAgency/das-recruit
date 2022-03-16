using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.UnitTests.Provider.Web.HardMocks;
using Esfa.Recruit.Provider.Web;
using Esfa.Recruit.Provider.Web.Orchestrators.Part2;
using Esfa.Recruit.Provider.Web.ViewModels.Part2.Considerations;
using Esfa.Recruit.Shared.Web.FeatureToggle;
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
    public class ConsiderationsOrchestratorTests
    {
        private ConsiderationsOrchestratorTestsFixture _fixture;

        public ConsiderationsOrchestratorTests()
        {
            _fixture = new ConsiderationsOrchestratorTestsFixture();
        }

        [Fact]
        public async Task WhenUpdated__ShouldCallUpdateDraftVacancy()
        {
            _fixture
                .WithThingsToConsider("has a value")
                .Setup();

            var thingsToConsiderEditModel = new ConsiderationsEditModel
            {
                Ukprn = _fixture.Vacancy.TrainingProvider.Ukprn.Value,
                VacancyId = _fixture.Vacancy.Id,
                ThingsToConsider = "has a new value"
            };

            await _fixture.PostConsiderationsEditModelAsync(thingsToConsiderEditModel);

            _fixture.VerifyUpdateDraftVacancyAsyncIsCalled();
        }

        [Fact]
        public async Task WhenThingsToConsiderIsUpdated_ShouldFlagThingsToConsiderFieldIndicator()
        {
            _fixture
                .WithThingsToConsider("has a value")
                .Setup();

            var thingsToConsiderEditModel = new ConsiderationsEditModel
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
            public ConsiderationsOrchestrator Sut {get; private set;}

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

                Sut = new ConsiderationsOrchestrator(Mock.Of<ILogger<ConsiderationsOrchestrator>>(), MockRecruitVacancyClient.Object, Mock.Of<IReviewSummaryService>(), new Utility(MockRecruitVacancyClient.Object, Mock.Of<IFeature>()));
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
