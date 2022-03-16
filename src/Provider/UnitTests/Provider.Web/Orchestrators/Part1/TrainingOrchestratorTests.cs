using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.UnitTests.Provider.Web.HardMocks;
using Esfa.Recruit.Provider.Web;
using Esfa.Recruit.Provider.Web.Orchestrators.Part1;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.Training;
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
    public class TrainingOrchestratorTests
    {
        private TrainingOrchestratorTestsFixture _fixture;

        public TrainingOrchestratorTests()
        {
            _fixture = new TrainingOrchestratorTestsFixture();
        }

        [Theory]
        [InlineData("this is a value", false)]
        [InlineData("this is a new value", true)]
        public async Task WhenUpdated_ShouldFlagFieldIndicators(string programmeId, bool fieldIndicatorSet)
        {
            _fixture
                .WithProgrammeId("this is a value")
                .Setup();

            var confirmTrainingEditModel = new ConfirmTrainingEditModel
            {
                Ukprn = _fixture.Vacancy.TrainingProvider.Ukprn.Value,
                VacancyId = _fixture.Vacancy.Id,
                ProgrammeId = programmeId
            };

            await _fixture.PostConfirmTrainingEditModelAsync(confirmTrainingEditModel);

            _fixture.VerifyProviderReviewFieldIndicators(FieldIdentifiers.TrainingLevel, fieldIndicatorSet);
            _fixture.VerifyProviderReviewFieldIndicators(FieldIdentifiers.Training, fieldIndicatorSet);
        }

        public class TrainingOrchestratorTestsFixture
        {
            private const VacancyRuleSet ValidationRules = VacancyRuleSet.TrainingProgramme;
            public VacancyUser User { get; }
            public Vacancy Vacancy { get; }
            public TrainingOrchestrator Sut {get; private set;}

            public TrainingOrchestratorTestsFixture()
            {
                MockClient = new Mock<IProviderVacancyClient>();
                MockRecruitVacancyClient = new Mock<IRecruitVacancyClient>();

                User = VacancyOrchestratorTestData.GetVacancyUser();
                Vacancy = VacancyOrchestratorTestData.GetPart1CompleteVacancy();
            }

            public TrainingOrchestratorTestsFixture WithProgrammeId(string programmeId)
            {
                Vacancy.ProgrammeId = programmeId;
                return this;
            }

            public void Setup()
            {
                MockRecruitVacancyClient.Setup(x => x.GetVacancyAsync(Vacancy.Id)).ReturnsAsync(Vacancy);
                MockRecruitVacancyClient.Setup(x => x.Validate(Vacancy, ValidationRules)).Returns(new EntityValidationResult());
                MockRecruitVacancyClient.Setup(x => x.UpdateDraftVacancyAsync(It.IsAny<Vacancy>(), User));
                MockRecruitVacancyClient.Setup(x => x.UpdateEmployerProfileAsync(It.IsAny<EmployerProfile>(), User));

                Sut = new TrainingOrchestrator(MockRecruitVacancyClient.Object, Mock.Of<ILogger<TrainingOrchestrator>>(), 
                    Mock.Of<IReviewSummaryService>(), new Utility(MockRecruitVacancyClient.Object, Mock.Of<IFeature>()));
            }

            public async Task PostConfirmTrainingEditModelAsync(ConfirmTrainingEditModel model)
            {
                await Sut.PostConfirmTrainingEditModelAsync(model, User);
            }

            public void VerifyProviderReviewFieldIndicators(string fieldIdentifier, bool value)
            {
                Vacancy.ProviderReviewFieldIndicators
                    .SingleOrDefault(p => p.FieldIdentifier == fieldIdentifier)
                    .Should().NotBeNull().And
                    .Match<ProviderReviewFieldIndicator>((x) => x.IsChangeRequested == value);
            }

            public Mock<IProviderVacancyClient> MockClient { get; set; }
            public Mock<IRecruitVacancyClient> MockRecruitVacancyClient { get; set; }
        }
    }
}
