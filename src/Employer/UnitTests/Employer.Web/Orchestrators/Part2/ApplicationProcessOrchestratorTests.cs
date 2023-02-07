using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.UnitTests.Employer.Web.HardMocks;
using Esfa.Recruit.Employer.Web;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Orchestrators.Part2;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Shared.Web.FeatureToggle;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Orchestrators.Part2
{
    public class ApplicationProcessOrchestratorTests
    {
        private ApplicationProcessOrchestratorTestsFixture _fixture;

        public ApplicationProcessOrchestratorTests()
        {
            _fixture = new ApplicationProcessOrchestratorTestsFixture();
        }

        [Fact]
        public async Task WhenApplicationMethodIsThroughFaaVacancy_ShouldCallUpdateDraftVacancy()
        {
            _fixture
                .WithApplicationMethod(ApplicationMethod.ThroughExternalApplicationSite)
                .WithApplicationInstructions("has a value")
                .WithApplicationUrl("has a value")
                .Setup();

            var applicationProcessEditModel = new ApplicationProcessEditModel
            {
                EmployerAccountId = _fixture.Vacancy.EmployerAccountId,
                VacancyId = _fixture.Vacancy.Id,
                ApplicationMethod = ApplicationMethod.ThroughFindAnApprenticeship,
                ApplicationInstructions = "has a value",
            };

            await _fixture.PostApplicationProcessEditModelAsync(applicationProcessEditModel);

            _fixture.VerifyUpdateDraftVacancyAsyncIsCalled();
        }

        [Fact]
        public async Task WhenApplicationMethodIsThroughFaaVacancy_ShouldOverwriteApplicationUrlAsNull()
        {
            _fixture
                .WithApplicationMethod(ApplicationMethod.ThroughExternalApplicationSite)
                .WithApplicationInstructions("has a value")
                .WithApplicationUrl("has a value")
                .Setup();

            var applicationProcessEditModel = new ApplicationProcessEditModel
            {
                EmployerAccountId = _fixture.Vacancy.EmployerAccountId,
                VacancyId = _fixture.Vacancy.Id,
                ApplicationMethod = ApplicationMethod.ThroughFindAnApprenticeship,
                ApplicationUrl = "has a value"
            };

            await _fixture.PostApplicationProcessEditModelAsync(applicationProcessEditModel);

            _fixture.VerifyApplicationMethod(ApplicationMethod.ThroughFindAnApprenticeship);
            _fixture.VerifyOverwriteApplicationUrlAsNull();
        }

        [Fact]
        public async Task WhenApplicationMethodIsThroughFaaVacancy_ShouldOverwriteApplicationInstructionsAsNull()
        {
            _fixture
                .WithApplicationMethod(ApplicationMethod.ThroughExternalApplicationSite)
                .WithApplicationInstructions("has a value")
                .WithApplicationUrl("has a value")
                .Setup();

            var applicationProcessEditModel = new ApplicationProcessEditModel
            {
                EmployerAccountId = _fixture.Vacancy.EmployerAccountId,
                VacancyId = _fixture.Vacancy.Id,
                ApplicationMethod = ApplicationMethod.ThroughFindAnApprenticeship,
                ApplicationInstructions = "has a value",
            };

            await _fixture.PostApplicationProcessEditModelAsync(applicationProcessEditModel);

            _fixture.VerifyApplicationMethod(ApplicationMethod.ThroughFindAnApprenticeship);
            _fixture.VerifyOverwriteApplicationInstructionsAsNull();
        }

        [Fact]
        public async Task WhenApplicationMethodIsThroughFaaVacancy_ShouldFlagAllApplicationFieldIndicators()
        {
            _fixture
                .WithApplicationMethod(ApplicationMethod.ThroughExternalApplicationSite)
                .WithApplicationInstructions("has a value")
                .WithApplicationUrl("has a value")
                .Setup();

            var applicationProcessEditModel = new ApplicationProcessEditModel
            {
                EmployerAccountId = _fixture.Vacancy.EmployerAccountId,
                VacancyId = _fixture.Vacancy.Id,
                ApplicationMethod = ApplicationMethod.ThroughFindAnApprenticeship,
                ApplicationInstructions = "has a value",
                ApplicationUrl = "has a value"
            };

            await _fixture.PostApplicationProcessEditModelAsync(applicationProcessEditModel);

            _fixture.VerifyEmployerReviewFieldIndicators(FieldIdentifiers.ApplicationMethod, true);
            _fixture.VerifyEmployerReviewFieldIndicators(FieldIdentifiers.ApplicationInstructions, true);
            _fixture.VerifyEmployerReviewFieldIndicators(FieldIdentifiers.ApplicationUrl, true);
        }

        [Fact]
        public async Task WhenApplicationInstructionsIsUpdated_ShouldFlagApplicationInstructionsFieldIndicator()
        {
            _fixture
                .WithApplicationMethod(ApplicationMethod.ThroughExternalApplicationSite)
                .WithApplicationInstructions("has a value")
                .WithApplicationUrl("has a value")
                .Setup();

            var applicationProcessEditModel = new ApplicationProcessEditModel
            {
                EmployerAccountId = _fixture.Vacancy.EmployerAccountId,
                VacancyId = _fixture.Vacancy.Id,
                ApplicationMethod = ApplicationMethod.ThroughExternalApplicationSite,
                ApplicationInstructions = "has a new value",
                ApplicationUrl = "has a value"
            };

            await _fixture.PostApplicationProcessEditModelAsync(applicationProcessEditModel);

            _fixture.VerifyEmployerReviewFieldIndicators(FieldIdentifiers.ApplicationMethod, false);
            _fixture.VerifyEmployerReviewFieldIndicators(FieldIdentifiers.ApplicationInstructions, true);
            _fixture.VerifyEmployerReviewFieldIndicators(FieldIdentifiers.ApplicationUrl, false);
        }

        [Fact]
        public async Task WhenApplicationUrlIsUpdated_ShouldFlagApplicationInstructionsFieldIndicator()
        {
            _fixture
                .WithApplicationMethod(ApplicationMethod.ThroughExternalApplicationSite)
                .WithApplicationInstructions("has a value")
                .WithApplicationUrl("has a value")
                .Setup();

            var applicationProcessEditModel = new ApplicationProcessEditModel
            {
                EmployerAccountId = _fixture.Vacancy.EmployerAccountId,
                VacancyId = _fixture.Vacancy.Id,
                ApplicationMethod = ApplicationMethod.ThroughExternalApplicationSite,
                ApplicationInstructions = "has a value",
                ApplicationUrl = "has a new value"
            };

            await _fixture.PostApplicationProcessEditModelAsync(applicationProcessEditModel);

            _fixture.VerifyEmployerReviewFieldIndicators(FieldIdentifiers.ApplicationMethod, false);
            _fixture.VerifyEmployerReviewFieldIndicators(FieldIdentifiers.ApplicationInstructions, false);
            _fixture.VerifyEmployerReviewFieldIndicators(FieldIdentifiers.ApplicationUrl, true);
        }

        public class ApplicationProcessOrchestratorTestsFixture
        {
            private const VacancyRuleSet ValidationRules = VacancyRuleSet.ApplicationMethod;
            public VacancyUser User { get; }
            public Vacancy Vacancy { get; }
            public ApplicationProcessOrchestrator Sut { get; private set; }

            public ApplicationProcessOrchestratorTestsFixture()
            {
                MockClient = new Mock<IEmployerVacancyClient>();
                MockRecruitVacancyClient = new Mock<IRecruitVacancyClient>();
                User = VacancyOrchestratorTestData.GetVacancyUser();
                Vacancy = VacancyOrchestratorTestData.GetPart1CompleteVacancy();
            }

            public ApplicationProcessOrchestratorTestsFixture WithApplicationMethod(ApplicationMethod applicationMethod)
            {
                Vacancy.ApplicationMethod = applicationMethod;
                return this;
            }

            public ApplicationProcessOrchestratorTestsFixture WithApplicationInstructions(string applicationInstructions)
            {
                Vacancy.ApplicationInstructions = applicationInstructions;
                return this;
            }

            public ApplicationProcessOrchestratorTestsFixture WithApplicationUrl(string applicationUrl)
            {
                Vacancy.ApplicationUrl = applicationUrl;
                return this;
            }

            public void Setup()
            {
                MockRecruitVacancyClient.Setup(x => x.GetVacancyAsync(Vacancy.Id)).ReturnsAsync(Vacancy);
                MockRecruitVacancyClient.Setup(x => x.Validate(Vacancy, ValidationRules)).Returns(new EntityValidationResult());
                MockRecruitVacancyClient.Setup(x => x.UpdateDraftVacancyAsync(It.IsAny<Vacancy>(), User));
                MockRecruitVacancyClient.Setup(x => x.UpdateEmployerProfileAsync(It.IsAny<EmployerProfile>(), User));
                var utility = new Utility(MockRecruitVacancyClient.Object);
                
                Sut = new ApplicationProcessOrchestrator(MockRecruitVacancyClient.Object, Options.Create(new ExternalLinksConfiguration()), Mock.Of<ILogger<ApplicationProcessOrchestrator>>(), Mock.Of<IReviewSummaryService>(), utility);
            }

            public async Task PostApplicationProcessEditModelAsync(ApplicationProcessEditModel model)
            {
                await Sut.PostApplicationProcessEditModelAsync(model, User);
            }

            public void VerifyEmployerReviewFieldIndicators(string fieldIdentifier, bool value)
            {
                Vacancy.EmployerReviewFieldIndicators
                    .Where(p => p.FieldIdentifier == fieldIdentifier).Single()
                    .Should().NotBeNull().And
                    .Match<EmployerReviewFieldIndicator>((x) => x.IsChangeRequested == value);
            }

            public void VerifyApplicationMethod(ApplicationMethod applicationMethod)
            {
                Vacancy.ApplicationMethod.HasValue.Should().BeTrue();
                Vacancy.ApplicationMethod.Value.Should().Be(applicationMethod);
            }

            public void VerifyOverwriteApplicationInstructionsAsNull()
            {
                Vacancy.ApplicationInstructions.Should().BeNull();
            }

            public void VerifyOverwriteApplicationUrlAsNull()
            {
                Vacancy.ApplicationUrl.Should().BeNull();
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
