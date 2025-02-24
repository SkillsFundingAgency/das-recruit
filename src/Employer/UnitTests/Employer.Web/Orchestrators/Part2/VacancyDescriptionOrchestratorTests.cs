using System.Linq;
using Esfa.Recruit.Employer.UnitTests.Employer.Web.HardMocks;
using Esfa.Recruit.Employer.Web;
using Esfa.Recruit.Employer.Web.Orchestrators.Part2;
using Esfa.Recruit.Employer.Web.ViewModels.Part2.VacancyDescription;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Orchestrators.Part2
{
    public class VacancyDescriptionOrchestratorTests
    {
        private VacancyDescriptionOrchestratorTestsFixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = new VacancyDescriptionOrchestratorTestsFixture();
        }

        [TestCase("has a new value", "has a value")]
        [TestCase("has a value", "has a new value")]
        [TestCase("has a value", "has a value")]
        [TestCase("has a new value", "has a new value")]
        public async Task WhenUpdated__ShouldCallUpdateDraftVacancy(string description, string trainingDescription)
        {
            _fixture
                .WithDescription("has a value")
                .WithTrainingDescription("has a value")
                .Setup();

            var vacancyDescriptionEditModel = new VacancyDescriptionEditModel
            {
                EmployerAccountId = _fixture.Vacancy.EmployerAccountId,
                VacancyId = _fixture.Vacancy.Id,
                VacancyDescription = description,
                TrainingDescription = trainingDescription
            };

            await _fixture.PostVacancyDescriptionEditModelAsync(vacancyDescriptionEditModel);

            _fixture.VerifyUpdateDraftVacancyAsyncIsCalled();
        }

        [TestCase("has a new value", "has a value", new string[] { FieldIdentifiers.VacancyDescription }, new string[] { FieldIdentifiers.TrainingDescription })]
        [TestCase("has a value", "has a new value", new string[] { FieldIdentifiers.TrainingDescription }, new string[] { FieldIdentifiers.VacancyDescription })]
        [TestCase("has a new value", "has a new value", new string[] { FieldIdentifiers.VacancyDescription, FieldIdentifiers.TrainingDescription }, new string[] { })]
        public async Task WhenShortDescriptionIsUpdated_ShouldFlagFieldIndicators(string description, string trainingDescription, string[] setFieldIndicators, string[] unsetFieldIndicators)
        {
            _fixture
                .WithDescription("has a value")
                .WithTrainingDescription("has a value")
                .Setup();

            var vacancyDescriptionEditModel = new VacancyDescriptionEditModel
            {
                EmployerAccountId = _fixture.Vacancy.EmployerAccountId,
                VacancyId = _fixture.Vacancy.Id,
                VacancyDescription = description,
                TrainingDescription = trainingDescription
            };

            await _fixture.PostVacancyDescriptionEditModelAsync(vacancyDescriptionEditModel);

            _fixture.VerifyEmployerReviewFieldIndicators(setFieldIndicators, unsetFieldIndicators);
        }

        public class VacancyDescriptionOrchestratorTestsFixture
        {
            private const VacancyRuleSet ValidationRules = VacancyRuleSet.Description | VacancyRuleSet.TrainingDescription;
            public VacancyUser User { get; }
            public Vacancy Vacancy { get; }
            public VacancyDescriptionOrchestrator Sut {get; private set;}

            public VacancyDescriptionOrchestratorTestsFixture()
            {
                MockClient = new Mock<IEmployerVacancyClient>();
                MockRecruitVacancyClient = new Mock<IRecruitVacancyClient>();

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

            public void Setup()
            {
                MockRecruitVacancyClient.Setup(x => x.GetVacancyAsync(Vacancy.Id)).ReturnsAsync(Vacancy);
                MockRecruitVacancyClient.Setup(x => x.Validate(Vacancy, ValidationRules)).Returns(new EntityValidationResult());
                MockRecruitVacancyClient.Setup(x => x.UpdateDraftVacancyAsync(It.IsAny<Vacancy>(), User));
                MockRecruitVacancyClient.Setup(x => x.UpdateEmployerProfileAsync(It.IsAny<EmployerProfile>(), User));
                var utility = new Utility(MockRecruitVacancyClient.Object);
                
                Sut = new VacancyDescriptionOrchestrator(MockRecruitVacancyClient.Object, Mock.Of<ILogger<VacancyDescriptionOrchestrator>>(), Mock.Of<IReviewSummaryService>(), utility);
            }

            public async Task PostVacancyDescriptionEditModelAsync(VacancyDescriptionEditModel model)
            {
                await Sut.PostVacancyDescriptionEditModelAsync(model, User);
            }

            public void VerifyEmployerReviewFieldIndicators(string[] setFieldIdentifiers, string[] unsetFieldIdentifiers)
            {
                foreach (var fieldIdentifier in setFieldIdentifiers)
                {
                    VerifyEmployerReviewFieldIndicators(fieldIdentifier, true);
                }

                foreach (var fieldIdentifier in unsetFieldIdentifiers)
                {
                    VerifyEmployerReviewFieldIndicators(fieldIdentifier, false);
                }
            }

            public void VerifyEmployerReviewFieldIndicators(string fieldIdentifier, bool value)
            {
                Vacancy.EmployerReviewFieldIndicators
                    .Where(p => p.FieldIdentifier == fieldIdentifier).Single()
                    .Should().NotBeNull().And
                    .Match<EmployerReviewFieldIndicator>((x) => x.IsChangeRequested == value);
            }

            public void VerifyUpdateDraftVacancyAsyncIsCalled()
            {
                MockRecruitVacancyClient.Verify(x => x.UpdateDraftVacancyAsync(Vacancy, User), Times.Once);
            }

            public Mock<IEmployerVacancyClient> MockClient { get; set; }
            public Mock<IRecruitVacancyClient> MockRecruitVacancyClient { get; set; }
        }
    }
}
