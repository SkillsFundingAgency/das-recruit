using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.UnitTests.Provider.Web.HardMocks;
using Esfa.Recruit.Provider.Web;
using Esfa.Recruit.Provider.Web.Orchestrators.Part1;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.NumberOfPositions;
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

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Orchestrators.Part1
{
    public class NumberofPositionsOrchestratorTests
    {
        private NumberofPositionsOrchestratorTestsFixture _fixture;

        public NumberofPositionsOrchestratorTests()
        {
            _fixture = new NumberofPositionsOrchestratorTestsFixture();
        }

        [Theory]
        [InlineData(1, false)]
        [InlineData(2, true)]
        public async Task WhenUpdated_ShouldFlagFieldIndicators(int numberOfPositions, bool fieldIndicatorSet)
        {
            _fixture
                .WithNumberOfPositions(1)
                .Setup();

            var numberOfPositionsEditModel = new NumberOfPositionsEditModel
            {
                Ukprn = _fixture.Vacancy.TrainingProvider.Ukprn.Value,
                VacancyId = _fixture.Vacancy.Id,
                NumberOfPositions = numberOfPositions.ToString()
            };

            await _fixture.PostNumberOfPositionsEditModelAsync(numberOfPositionsEditModel);

            _fixture.VerifyProviderReviewFieldIndicators(FieldIdentifiers.NumberOfPositions, fieldIndicatorSet);
        }

        public class NumberofPositionsOrchestratorTestsFixture
        {
            private const VacancyRuleSet ValidationRules = VacancyRuleSet.NumberOfPositions;
            public VacancyUser User { get; }
            public Vacancy Vacancy { get; }
            public NumberOfPositionsOrchestrator Sut {get; private set;}

            public NumberofPositionsOrchestratorTestsFixture()
            {
                MockClient = new Mock<IProviderVacancyClient>();
                MockRecruitVacancyClient = new Mock<IRecruitVacancyClient>();

                User = VacancyOrchestratorTestData.GetVacancyUser();
                Vacancy = VacancyOrchestratorTestData.GetPart1CompleteVacancy();
            }

            public NumberofPositionsOrchestratorTestsFixture WithNumberOfPositions(int numberOfPositions)
            {
                Vacancy.NumberOfPositions = numberOfPositions;
                return this;
            }

            public void Setup()
            {
                MockRecruitVacancyClient.Setup(x => x.GetVacancyAsync(Vacancy.Id)).ReturnsAsync(Vacancy);
                MockRecruitVacancyClient.Setup(x => x.Validate(Vacancy, ValidationRules)).Returns(new EntityValidationResult());
                MockRecruitVacancyClient.Setup(x => x.UpdateDraftVacancyAsync(It.IsAny<Vacancy>(), User));
                MockRecruitVacancyClient.Setup(x => x.UpdateEmployerProfileAsync(It.IsAny<EmployerProfile>(), User));

                Sut = new NumberOfPositionsOrchestrator(MockRecruitVacancyClient.Object, Mock.Of<ILogger<NumberOfPositionsOrchestrator>>(), 
                    Mock.Of<IReviewSummaryService>(), new Utility(MockRecruitVacancyClient.Object,Mock.Of<IFeature>()));
            }

            public async Task PostNumberOfPositionsEditModelAsync(NumberOfPositionsEditModel model)
            {
                await Sut.PostNumberOfPositionsEditModelAsync(model, User);
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
