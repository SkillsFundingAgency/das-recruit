using System.Linq;
using Esfa.Recruit.Employer.UnitTests.Employer.Web.HardMocks;
using Esfa.Recruit.Employer.Web;
using Esfa.Recruit.Employer.Web.Orchestrators.Part1;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.CustomWage;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Orchestrators.Part1
{
    public class CustomWageOrchestratorTests
    {
        private CustomWageOrchestratorTestsFixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = new CustomWageOrchestratorTestsFixture();
        }


        [TestCase(11000, true)]
        [TestCase(10000, false)]
        public async Task WhenUpdated_ShouldFlagFieldIndicators(decimal fixedWageYearlyAmount, bool fieldIndicatorSet)
        {
            _fixture
                .WithWageType(WageType.FixedWage)
                .WithFixedWageYearlyAmount(10000)
                .Setup();

            var wageEditModel = new CustomWageEditModel
            {
                EmployerAccountId = _fixture.Vacancy.EmployerAccountId,
                VacancyId = _fixture.Vacancy.Id,
                FixedWageYearlyAmount = fixedWageYearlyAmount.ToString()
            };

            await _fixture.PostCustomWageEditModelAsync(wageEditModel);

            _fixture.VerifyEmployerReviewFieldIndicators(FieldIdentifiers.Wage, fieldIndicatorSet);
        }

        public class CustomWageOrchestratorTestsFixture
        {
            private const VacancyRuleSet ValidationRules = VacancyRuleSet.Wage | VacancyRuleSet.MinimumWage;
            public VacancyUser User { get; }
            public Vacancy Vacancy { get; }
            public CustomWageOrchestrator Sut {get; private set;}

            public CustomWageOrchestratorTestsFixture()
            {
                MockClient = new Mock<IEmployerVacancyClient>();
                MockRecruitVacancyClient = new Mock<IRecruitVacancyClient>();

                User = VacancyOrchestratorTestData.GetVacancyUser();
                Vacancy = VacancyOrchestratorTestData.GetPart1CompleteVacancy();
            }

            public CustomWageOrchestratorTestsFixture WithWageType(WageType wageType)
            {
                Vacancy.Wage.WageType = wageType;
                return this;
            }

            public CustomWageOrchestratorTestsFixture WithFixedWageYearlyAmount(decimal fixedWageYearlyAmmount)
            {
                Vacancy.Wage.FixedWageYearlyAmount = fixedWageYearlyAmmount;
                return this;
            }

            public void Setup()
            {
                MockRecruitVacancyClient.Setup(x => x.GetVacancyAsync(Vacancy.Id)).ReturnsAsync(Vacancy);
                MockRecruitVacancyClient.Setup(x => x.Validate(Vacancy, ValidationRules)).Returns(new EntityValidationResult());
                MockRecruitVacancyClient.Setup(x => x.UpdateDraftVacancyAsync(It.IsAny<Vacancy>(), User));
                MockRecruitVacancyClient.Setup(x => x.UpdateEmployerProfileAsync(It.IsAny<EmployerProfile>(), User));
                var utility = new Utility(MockRecruitVacancyClient.Object);
                
                Sut = new CustomWageOrchestrator(MockRecruitVacancyClient.Object, Mock.Of<ILogger<WageOrchestrator>>(), 
                    Mock.Of<IReviewSummaryService>(), Mock.Of<IMinimumWageProvider>(), utility);
            }

            public async Task PostCustomWageEditModelAsync(CustomWageEditModel model)
            {
                await Sut.PostCustomWageEditModelAsync(model, User);
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
