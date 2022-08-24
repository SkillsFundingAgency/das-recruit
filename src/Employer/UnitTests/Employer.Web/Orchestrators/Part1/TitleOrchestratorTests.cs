using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.UnitTests.Employer.Web.HardMocks;
using Esfa.Recruit.Employer.Web;
using Esfa.Recruit.Employer.Web.Orchestrators.Part1;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Title;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.TrainingProvider;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Orchestrators.Part1
{
    public class TitleOrchestratorTests
    {
        private TitleOrchestratorTestsFixture _fixture;

        public TitleOrchestratorTests()
        {
            _fixture = new TitleOrchestratorTestsFixture();
        }

        [Theory]
        [InlineData("this is a value", false)]
        [InlineData("this is a new value", true)]
        public async Task WhenUpdated_ShouldFlagFieldIndicators(string title, bool fieldIndicatorSet)
        {
            _fixture
                .WithTitle("this is a value")
                .Setup();

            var titleEditModel = new TitleEditModel
            {
                EmployerAccountId = _fixture.Vacancy.EmployerAccountId,
                VacancyId = _fixture.Vacancy.Id,
                Title = title
            };

            await _fixture.PostTitleEditModelAsync(titleEditModel);

            _fixture.VerifyEmployerReviewFieldIndicators(FieldIdentifiers.Title, fieldIndicatorSet);
        }

        public class TitleOrchestratorTestsFixture
        {
            private const VacancyRuleSet ValidationRules = VacancyRuleSet.Title;
            public VacancyUser User { get; }
            public Vacancy Vacancy { get; }
            public TitleOrchestrator Sut {get; private set;}

            public TitleOrchestratorTestsFixture()
            {
                MockClient = new Mock<IEmployerVacancyClient>();
                MockRecruitVacancyClient = new Mock<IRecruitVacancyClient>();

                User = VacancyOrchestratorTestData.GetVacancyUser();
                Vacancy = VacancyOrchestratorTestData.GetPart1CompleteVacancy();
            }

            public TitleOrchestratorTestsFixture WithTitle(string title)
            {
                Vacancy.Title = title;
                return this;
            }

            public void Setup()
            {
                MockRecruitVacancyClient.Setup(x => x.GetVacancyAsync(Vacancy.Id)).ReturnsAsync(Vacancy);
                MockRecruitVacancyClient.Setup(x => x.Validate(Vacancy, ValidationRules)).Returns(new EntityValidationResult());
                MockRecruitVacancyClient.Setup(x => x.UpdateDraftVacancyAsync(It.IsAny<Vacancy>(), User));
                MockRecruitVacancyClient.Setup(x => x.UpdateEmployerProfileAsync(It.IsAny<EmployerProfile>(), User));
                var utility = new Utility(MockRecruitVacancyClient.Object);
                
                Sut = new TitleOrchestrator(MockClient.Object, MockRecruitVacancyClient.Object, Mock.Of<ILogger<TitleOrchestrator>>(), 
                    Mock.Of<IReviewSummaryService>(), Mock.Of<ITrainingProviderService>(), utility);
            }

            public async Task PostTitleEditModelAsync(TitleEditModel model)
            {
                await Sut.PostTitleEditModelAsync(model, User);
            }

            public void VerifyEmployerReviewFieldIndicators(string fieldIdentifier, bool value)
            {
                Vacancy.EmployerReviewFieldIndicators
                    .Where(p => p.FieldIdentifier == fieldIdentifier).Single()
                    .Should().NotBeNull().And
                    .Match<EmployerReviewFieldIndicator>((x) => x.IsChangeRequested == value);
            }

            public Mock<IEmployerVacancyClient> MockClient { get; set; }
            public Mock<IRecruitVacancyClient> MockRecruitVacancyClient { get; set; }
        }
    }
}
