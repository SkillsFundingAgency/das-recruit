using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.UnitTests.Employer.Web.HardMocks;
using Esfa.Recruit.Employer.Web.Orchestrators.Part2;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Orchestrators.Part2
{
    public class AboutEmployerOrchestratorTests
    {
        private AboutEmployerOrchestratorTestsFixture _fixture;

        public AboutEmployerOrchestratorTests()
        {
            _fixture = new AboutEmployerOrchestratorTestsFixture();
        }

        [Fact]
        public async Task WhenEmployerDescriptionIsUpdated_ShouldFlagEmployerDescriptionFieldIndicator()
        {
            _fixture
                .WithEmployerDescription("has a value")
                .WithEmployerWebsiteUrl("has a value")
                .Setup();

            var aboutEmployerEditModel = new AboutEmployerEditModel
            {
                EmployerAccountId = _fixture.Vacancy.EmployerAccountId,
                VacancyId = _fixture.Vacancy.Id,
                EmployerDescription = "has a new value",
                EmployerWebsiteUrl = "has a value"
            };

            await _fixture.PostAboutEmployerEditModelAsync(aboutEmployerEditModel);

            _fixture.VerifyEmployerReviewFieldIndicators(FieldIdentifiers.EmployerDescription, true);
            _fixture.VerifyEmployerReviewFieldIndicators(FieldIdentifiers.EmployerWebsiteUrl, false);
        }

        [Fact]
        public async Task WhenEmployerWebsiteUrlIsUpdated_ShouldFlagEmployerWebsiteUrlFieldIndicator()
        {
            _fixture
                .WithEmployerDescription("has a value")
                .WithEmployerWebsiteUrl("has a value")
                .Setup();

            var aboutEmployerEditModel = new AboutEmployerEditModel
            {
                EmployerAccountId = _fixture.Vacancy.EmployerAccountId,
                VacancyId = _fixture.Vacancy.Id,
                EmployerDescription = "has a value",
                EmployerWebsiteUrl = "has a new value"
            };

            await _fixture.PostAboutEmployerEditModelAsync(aboutEmployerEditModel);

            _fixture.VerifyEmployerReviewFieldIndicators(FieldIdentifiers.EmployerDescription, false);
            _fixture.VerifyEmployerReviewFieldIndicators(FieldIdentifiers.EmployerWebsiteUrl, true);
        }

        public class AboutEmployerOrchestratorTestsFixture
        {
            private const VacancyRuleSet ValidationRules = VacancyRuleSet.EmployerDescription | VacancyRuleSet.EmployerWebsiteUrl;
            public VacancyUser User { get; }
            public EmployerProfile EmployerProfile { get; }
            public Vacancy Vacancy { get; }
            public AboutEmployerOrchestrator Sut {get; private set;}

            public AboutEmployerOrchestratorTestsFixture()
            {
                MockClient = new Mock<IEmployerVacancyClient>();
                MockRecruitVacancyClient = new Mock<IRecruitVacancyClient>();

                User = VacancyOrchestratorTestData.GetVacancyUser();
                EmployerProfile = VacancyOrchestratorTestData.GetEmployerProfile();
                Vacancy = VacancyOrchestratorTestData.GetPart1CompleteVacancy();
            }

            public AboutEmployerOrchestratorTestsFixture WithEmployerDescription(string employerDescription)
            {
                Vacancy.EmployerDescription = employerDescription;
                return this;
            }

            public AboutEmployerOrchestratorTestsFixture WithEmployerWebsiteUrl(string employerWebsiteUrl)
            {
                Vacancy.EmployerWebsiteUrl = employerWebsiteUrl;
                return this;
            }

            public void Setup()
            {
                MockRecruitVacancyClient.Setup(x => x.GetVacancyAsync(Vacancy.Id)).ReturnsAsync(Vacancy);
                MockRecruitVacancyClient.Setup(x => x.GetEmployerProfileAsync(Vacancy.EmployerAccountId, Vacancy.AccountLegalEntityPublicHashedId)).ReturnsAsync(EmployerProfile);
                MockRecruitVacancyClient.Setup(x => x.Validate(Vacancy, ValidationRules)).Returns(new EntityValidationResult());
                MockRecruitVacancyClient.Setup(x => x.UpdateDraftVacancyAsync(It.IsAny<Vacancy>(), User));
                MockRecruitVacancyClient.Setup(x => x.UpdateEmployerProfileAsync(It.IsAny<EmployerProfile>(), User));

                Sut = new AboutEmployerOrchestrator(MockClient.Object, MockRecruitVacancyClient.Object, Mock.Of<ILogger<AboutEmployerOrchestrator>>(), Mock.Of<IReviewSummaryService>());
            }

            public async Task PostAboutEmployerEditModelAsync(AboutEmployerEditModel model)
            {
                await Sut.PostAboutEmployerEditModelAsync(model, User);
            }

            public void VerifyEmployerReviewFieldIndicators(string fieldIdentifier, bool value)
            {
                Vacancy.EmployerReviewFieldIndicators
                    .Where(p => p.FieldIdentifier == fieldIdentifier).FirstOrDefault()
                    .Should().NotBeNull().And
                    .Match<EmployerReviewFieldIndicator>((x) => x.IsChangeRequested == value);
            }

            public Mock<IEmployerVacancyClient> MockClient { get; set; }
            public Mock<IRecruitVacancyClient> MockRecruitVacancyClient { get; set; }
        }
    }
}
