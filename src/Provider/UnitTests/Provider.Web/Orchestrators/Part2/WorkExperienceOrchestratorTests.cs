using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Esfa.Recruit.Provider.UnitTests.Provider.Web.HardMocks;
using Esfa.Recruit.Provider.Web;
using Esfa.Recruit.Provider.Web.Orchestrators.Part2;
using Esfa.Recruit.Provider.Web.ViewModels.Part2.WorkExperience;
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
    public class WorkExperienceOrchestratorTests
    {
        private WorkExperienceOrchestratorTestsFixture _fixture;

        public WorkExperienceOrchestratorTests()
        {
            _fixture = new WorkExperienceOrchestratorTestsFixture();
        }
        
        [Fact]
        public async Task WhenUpdated_ShouldCallUpdateDraftVacancy_AndFlagWorkExperienceFieldIndicator()
        {
            _fixture
                .WithWorkExperience(_fixture.Fixture.Create<string>())
                .Setup();

            var workExperienceEditModel = new WorkExperienceEditModel
            {
                Ukprn = _fixture.Vacancy.TrainingProvider.Ukprn!.Value,
                VacancyId = _fixture.Vacancy.Id,
                WorkExperience = _fixture.Fixture.Create<string>()
            };

            await _fixture.PostWorkExperienceEditModelAsync(workExperienceEditModel);

            _fixture.VerifyUpdateDraftVacancyAsyncIsCalled();
            _fixture.VerifyProviderReviewFieldIndicators(FieldIdentifiers.WorkExperience, true);
        }
    }
    
    public class WorkExperienceOrchestratorTestsFixture
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.WorkExperience;
        public VacancyUser User { get; }
        public Vacancy Vacancy { get; }
        public WorkExperienceOrchestrator Sut { get; private set; }
        public Mock<IProviderVacancyClient> MockClient { get; set; }
        public Mock<IRecruitVacancyClient> MockRecruitVacancyClient { get; set; }
        public Fixture Fixture { get; }

        public WorkExperienceOrchestratorTestsFixture()
        {
            MockClient = new Mock<IProviderVacancyClient>();
            MockRecruitVacancyClient = new Mock<IRecruitVacancyClient>();

            User = VacancyOrchestratorTestData.GetVacancyUser();
            Vacancy = VacancyOrchestratorTestData.GetPart1CompleteVacancy();

            Fixture = new Fixture();
        }

        public WorkExperienceOrchestratorTestsFixture WithWorkExperience(string workExperience)
        {
            Vacancy.WorkExperience = workExperience;
            return this;
        }

        public void Setup()
        {
            MockRecruitVacancyClient.Setup(x => x.GetVacancyAsync(Vacancy.Id)).ReturnsAsync(Vacancy);
            MockRecruitVacancyClient.Setup(x => x.Validate(Vacancy, ValidationRules)).Returns(new EntityValidationResult());
            MockRecruitVacancyClient.Setup(x => x.UpdateDraftVacancyAsync(It.IsAny<Vacancy>(), User));
            MockRecruitVacancyClient.Setup(x => x.UpdateEmployerProfileAsync(It.IsAny<EmployerProfile>(), User));

            var mockUtility = new Utility(MockRecruitVacancyClient.Object);
            
            Sut = new WorkExperienceOrchestrator(MockRecruitVacancyClient.Object,
                Mock.Of<ILogger<WorkExperienceOrchestrator>>(), Mock.Of<IReviewSummaryService>(), mockUtility);
        }

        public async Task PostWorkExperienceEditModelAsync(WorkExperienceEditModel model)
        {
            await Sut.PostWorkExperienceEditModelAsync(model, User);
        }

        public void VerifyProviderReviewFieldIndicators(string fieldIdentifier, bool value)
        {
            Vacancy.ProviderReviewFieldIndicators
                .FirstOrDefault(p => p.FieldIdentifier == fieldIdentifier)
                .Should().NotBeNull()
                .And.Match<ProviderReviewFieldIndicator>((x) => x.IsChangeRequested == value);
        }

        public void VerifyUpdateDraftVacancyAsyncIsCalled()
        {
            MockRecruitVacancyClient.Verify(x => x.UpdateDraftVacancyAsync(Vacancy, User), Times.Once);
        }
    }
}