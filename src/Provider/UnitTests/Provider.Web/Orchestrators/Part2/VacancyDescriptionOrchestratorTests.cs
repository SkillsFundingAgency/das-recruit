using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.UnitTests.Provider.Web.HardMocks;
using Esfa.Recruit.Provider.Web;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Orchestrators.Part2;
using Esfa.Recruit.Provider.Web.ViewModels.Part2.VacancyDescription;
using Esfa.Recruit.Shared.Web.FeatureToggle;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Orchestrators.Part2
{
    public class VacancyDescriptionOrchestratorTests
    {
        private VacancyDescriptionOrchestratorTestsFixture _fixture;

        public VacancyDescriptionOrchestratorTests()
        {
            _fixture = new VacancyDescriptionOrchestratorTestsFixture();
        }

        [Theory]
        [InlineData("has a new value", "has a value")]
        [InlineData("has a value", "has a new value")]
        [InlineData("has a value", "has a value")]
        [InlineData("has a new value", "has a new value")]
        public async Task WhenUpdated__ShouldCallUpdateDraftVacancy(string description, string trainingDescription)
        {
            _fixture
                .WithTaskListSet(true)
                .WithDescription("has a value")
                .WithTrainingDescription("has a value")
                .Setup();

            var vacancyDescriptionEditModel = new VacancyDescriptionEditModel
            {
                Ukprn = _fixture.Vacancy.TrainingProvider.Ukprn.Value,
                VacancyId = _fixture.Vacancy.Id,
                VacancyDescription = description,
                TrainingDescription = trainingDescription,
            };

            await _fixture.PostVacancyDescriptionEditModelAsync(vacancyDescriptionEditModel);

            _fixture.VerifyUpdateDraftVacancyAsyncIsCalled();
        }
        
        [Fact]
        public async Task WhenUpdated_TaskListNotEnabled_ShouldCallUpdateDraftVacancy()
        {
            _fixture
                .WithTaskListSet(false)
                .WithOutcomeDescription("has a value")
                .Setup();

            var vacancyDescriptionEditModel = new VacancyDescriptionEditModel
            {
                Ukprn = _fixture.Vacancy.TrainingProvider.Ukprn.Value,
                VacancyId = _fixture.Vacancy.Id,
                OutcomeDescription = "has a value"
            };

            await _fixture.PostVacancyDescriptionEditModelAsync(vacancyDescriptionEditModel);

            _fixture.VerifyUpdateDraftVacancyAsyncIsCalled();
        }

        [Theory]
        [InlineData("has a new value", "has a value", new string[] { FieldIdentifiers.VacancyDescription }, new string[] { FieldIdentifiers.TrainingDescription})]
        [InlineData("has a value", "has a new value", new string[] { FieldIdentifiers.TrainingDescription }, new string[] { FieldIdentifiers.VacancyDescription})]
        [InlineData("has a new value", "has a new value", new string[] { FieldIdentifiers.VacancyDescription, FieldIdentifiers.TrainingDescription}, new string[] { })]
        public async Task WhenDescriptionIsUpdated_ShouldFlagFieldIndicators(string description, string trainingDescription, string[] setFieldIndicators, string[] unsetFieldIndicators)
        {
            _fixture
                .WithTaskListSet(true)
                .WithDescription("has a value")
                .WithTrainingDescription("has a value")
                .Setup();

            var vacancyDescriptionEditModel = new VacancyDescriptionEditModel
            {
                Ukprn = _fixture.Vacancy.TrainingProvider.Ukprn.Value,
                VacancyId = _fixture.Vacancy.Id,
                VacancyDescription = description,
                TrainingDescription = trainingDescription,
            };

            await _fixture.PostVacancyDescriptionEditModelAsync(vacancyDescriptionEditModel);

            _fixture.VerifyProviderReviewFieldIndicators(setFieldIndicators, unsetFieldIndicators);
        }
        
        [Fact]
        public async Task When_Traineeship_Then_Ignores_VacancyDescription_And_OutcomeDescription()
        {
            _fixture
                .WithTaskListSet(false)
                .WithVacancyType(VacancyType.Traineeship)
                .WithTrainingDescription("hello")
                .Setup();
            
            var vacancyDescriptionEditModel = new VacancyDescriptionEditModel
            {
                Ukprn = _fixture.Vacancy.TrainingProvider.Ukprn.Value,
                VacancyId = _fixture.Vacancy.Id,
                TrainingDescription = "new value"
            };
            
            await _fixture.PostVacancyDescriptionEditModelAsync(vacancyDescriptionEditModel);

            _fixture.VerifyProviderReviewFieldIndicators(
                new [] { FieldIdentifiers.TrainingDescription },
                new []{ FieldIdentifiers.VacancyDescription, FieldIdentifiers.OutcomeDescription});
        }

        [Fact]
        public async Task When_TaskList_Not_Enabled_Then_Sets_Review_Field_For_OutcomeDescription()
        {
            _fixture
                .WithTaskListSet(false)
                .WithOutcomeDescription("has a value")
                .Setup();
            
            var vacancyDescriptionEditModel = new VacancyDescriptionEditModel
            {
                Ukprn = _fixture.Vacancy.TrainingProvider.Ukprn.Value,
                VacancyId = _fixture.Vacancy.Id,
                OutcomeDescription = "has a new value"
            };
            
            await _fixture.PostVacancyDescriptionEditModelAsync(vacancyDescriptionEditModel);

            _fixture.VerifyProviderReviewFieldIndicators(new []{ FieldIdentifiers.OutcomeDescription},new []{ FieldIdentifiers.VacancyDescription, FieldIdentifiers.TrainingDescription});
        }

        public class VacancyDescriptionOrchestratorTestsFixture
        {
            private VacancyRuleSet ValidationRules;
            public VacancyUser User { get; }
            public Vacancy Vacancy { get; }
            public VacancyDescriptionOrchestrator Sut {get; private set;}

            public VacancyDescriptionOrchestratorTestsFixture()
            {
                MockClient = new Mock<IProviderVacancyClient>();
                MockRecruitVacancyClient = new Mock<IRecruitVacancyClient>();
                MockFeature = new Mock<IFeature>();

                User = VacancyOrchestratorTestData.GetVacancyUser();
                Vacancy = VacancyOrchestratorTestData.GetPart1CompleteVacancy();
            }

            public VacancyDescriptionOrchestratorTestsFixture WithDescription(string description)
            {
                Vacancy.Description = description;
                return this;
            }
            
            public VacancyDescriptionOrchestratorTestsFixture WithTrainingDescription(string trainingDescription)
            {
                Vacancy.TrainingDescription = trainingDescription;
                return this;
            }
            
            public VacancyDescriptionOrchestratorTestsFixture WithOutcomeDescription(string outcomeDescription)
            {
                Vacancy.OutcomeDescription = outcomeDescription;
                return this;
            }

            public VacancyDescriptionOrchestratorTestsFixture WithTaskListSet(bool value)
            {
                if (value)
                {
                    ValidationRules = VacancyRuleSet.Description | VacancyRuleSet.TrainingDescription;
                }
                else
                {
                    ValidationRules = VacancyRuleSet.Description | VacancyRuleSet.TrainingDescription |
                                      VacancyRuleSet.OutcomeDescription;
                }
                MockFeature.Setup(x => x.IsFeatureEnabled(FeatureNames.ProviderTaskList)).Returns(value);
                return this;
            }
            
            public VacancyDescriptionOrchestratorTestsFixture WithVacancyType(VacancyType vacancyType)
            {
                Vacancy.VacancyType = vacancyType;
                return this;
            }
            
            public void Setup()
            {
                MockRecruitVacancyClient.Setup(x => x.GetVacancyAsync(Vacancy.Id)).ReturnsAsync(Vacancy);
                MockRecruitVacancyClient.Setup(x => x.Validate(Vacancy, ValidationRules)).Returns(new EntityValidationResult());
                MockRecruitVacancyClient.Setup(x => x.UpdateDraftVacancyAsync(It.IsAny<Vacancy>(), User));
                MockRecruitVacancyClient.Setup(x => x.UpdateEmployerProfileAsync(It.IsAny<EmployerProfile>(), User));

                Sut = new VacancyDescriptionOrchestrator(MockRecruitVacancyClient.Object,
                    Mock.Of<ILogger<VacancyDescriptionOrchestrator>>(), Mock.Of<IReviewSummaryService>(),
                    new Utility(MockRecruitVacancyClient.Object), MockFeature.Object, new ServiceParameters(VacancyType.Apprenticeship.ToString()));
            }

            public async Task PostVacancyDescriptionEditModelAsync(VacancyDescriptionEditModel model)
            {
                await Sut.PostVacancyDescriptionEditModelAsync(model, User);
            }

            public void VerifyProviderReviewFieldIndicators(string[] setFieldIdentifiers, string[] unsetFieldIdentifiers)
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

            public void VerifyUpdateDraftVacancyAsyncIsCalled()
            {
                MockRecruitVacancyClient.Verify(x => x.UpdateDraftVacancyAsync(Vacancy, User), Times.Once);
            }

            public Mock<IProviderVacancyClient> MockClient { get; set; }
            public Mock<IRecruitVacancyClient> MockRecruitVacancyClient { get; set; }
            public Mock<IFeature> MockFeature { get; set; }
        }
    }
}
