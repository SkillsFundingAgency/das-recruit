using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.UnitTests.Provider.Web.HardMocks;
using Esfa.Recruit.Provider.Web;
using Esfa.Recruit.Provider.Web.Orchestrators.Part2;
using Esfa.Recruit.Provider.Web.ViewModels.Part2.AboutEmployer;
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
    public class AboutEmployerOrchestratorTests
    {
        private AboutEmployerOrchestratorTestsFixture _fixture;

        public AboutEmployerOrchestratorTests()
        {
            _fixture = new AboutEmployerOrchestratorTestsFixture();
        }

        [Theory]
        [InlineData("has a new value", "has a value", true, new string[] { FieldIdentifiers.EmployerDescription }, new string[] { FieldIdentifiers.EmployerWebsiteUrl})]
        [InlineData("has a value", "has a new value", true, new string[] { FieldIdentifiers.EmployerWebsiteUrl }, new string[] { FieldIdentifiers.EmployerDescription })]
        [InlineData("has a value", "has a value", false, new string[] { FieldIdentifiers.DisabilityConfident }, new string[] {FieldIdentifiers.EmployerDescription, FieldIdentifiers.EmployerWebsiteUrl })]
        [InlineData("has a new value", "has a new value", false, new string[] { FieldIdentifiers.EmployerDescription, FieldIdentifiers.EmployerWebsiteUrl, FieldIdentifiers.DisabilityConfident }, new string[] { })]
        
        public async Task WhenUpdated_ShouldFlagFieldIndicators(string employerDescription, string employerWebSiteUrl, bool disabilityConfident, string[] setFieldIdentifers, string [] unsetFieldIdentifiers)
        {
            _fixture
                .WithEmployerDescription("has a value")
                .WithEmployerWebsiteUrl("has a value")
                .WithDisabilityConfidentFlag(true)
                .Setup();

            var aboutEmployerEditModel = new AboutEmployerEditModel
            {
                Ukprn = _fixture.Vacancy.TrainingProvider.Ukprn.Value,
                VacancyId = _fixture.Vacancy.Id,
                EmployerDescription = employerDescription,
                EmployerWebsiteUrl = employerWebSiteUrl,
                IsDisabilityConfident = disabilityConfident
            };

            await _fixture.PostAboutEmployerEditModelAsync(aboutEmployerEditModel);

            _fixture.VerifyEmployerReviewFieldIndicators(setFieldIdentifers, unsetFieldIdentifiers);
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
                MockClient = new Mock<IProviderVacancyClient>();
                MockRecruitVacancyClient = new Mock<IRecruitVacancyClient>();

                User = VacancyOrchestratorTestData.GetVacancyUser();
                Vacancy = VacancyOrchestratorTestData.GetPart1CompleteVacancy();
                EmployerProfile = VacancyOrchestratorTestData.GetEmployerProfile(Vacancy.AccountLegalEntityPublicHashedId);
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

            public AboutEmployerOrchestratorTestsFixture WithDisabilityConfidentFlag(bool disabilityConfident)
            {
                Vacancy.DisabilityConfident = disabilityConfident ? DisabilityConfident.Yes : DisabilityConfident.No;
                return this;
            }

            public void Setup()
            {
                MockRecruitVacancyClient.Setup(x => x.GetVacancyAsync(Vacancy.Id)).ReturnsAsync(Vacancy);
                MockRecruitVacancyClient.Setup(x => x.GetEmployerProfileAsync(Vacancy.EmployerAccountId, Vacancy.AccountLegalEntityPublicHashedId)).ReturnsAsync(EmployerProfile);
                MockRecruitVacancyClient.Setup(x => x.Validate(Vacancy, ValidationRules)).Returns(new EntityValidationResult());
                MockRecruitVacancyClient.Setup(x => x.UpdateDraftVacancyAsync(It.IsAny<Vacancy>(), User));
                MockRecruitVacancyClient.Setup(x => x.UpdateEmployerProfileAsync(It.IsAny<EmployerProfile>(), User));

                Sut = new AboutEmployerOrchestrator(MockRecruitVacancyClient.Object, Mock.Of<ILogger<AboutEmployerOrchestrator>>(), Mock.Of<IReviewSummaryService>(), new Utility(MockRecruitVacancyClient.Object, Mock.Of<IFeature>()));
            }

            public async Task PostAboutEmployerEditModelAsync(AboutEmployerEditModel model)
            {
                await Sut.PostAboutEmployerEditModelAsync(model, User);
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
