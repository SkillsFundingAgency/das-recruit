using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.UnitTests.Employer.Web.HardMocks;
using Esfa.Recruit.Employer.Web.Orchestrators.Part1;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Duration;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Wage;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Orchestrators.Part1
{
    public class WageOrchestratorTests
    {
        private WageOrchestratorTestsFixture _fixture;

        public WageOrchestratorTests()
        {
            _fixture = new WageOrchestratorTestsFixture();
        }

        [Theory]
        [InlineData(WageType.FixedWage, 10000, "this is a value", false)]
        [InlineData(WageType.NationalMinimumWage, 10000, "this is a value", true)]
        [InlineData(WageType.FixedWage, 11000, "this is a value", true)]
        [InlineData(WageType.FixedWage, 10000, "this is a new value", true)]
        public async Task WhenUpdated_ShouldFlagFieldIndicators(WageType wageType, decimal fixedWageYearlyAmmount, string wageAddtionalInformation, bool fieldIndicatorSet)
        {
            _fixture
                .WithWageType(WageType.FixedWage)
                .WithFixedWageYearlyAmount(10000)
                .WithWageAdditionalInformation("this is a value")
                .Setup();

            var wageEditModel = new WageEditModel
            {
                EmployerAccountId = _fixture.Vacancy.EmployerAccountId,
                VacancyId = _fixture.Vacancy.Id,
                WageType = wageType,
                FixedWageYearlyAmount = fixedWageYearlyAmmount.ToString(),
                WageAdditionalInformation = wageAddtionalInformation
            };

            await _fixture.PostWageEditModelAsync(wageEditModel);

            _fixture.VerifyEmployerReviewFieldIndicators(FieldIdentifiers.Wage, fieldIndicatorSet);
        }

        public class WageOrchestratorTestsFixture
        {
            private const VacancyRuleSet ValidationRules = VacancyRuleSet.Wage | VacancyRuleSet.MinimumWage;
            public VacancyUser User { get; }
            public Vacancy Vacancy { get; }
            public WageOrchestrator Sut {get; private set;}

            public WageOrchestratorTestsFixture()
            {
                MockClient = new Mock<IEmployerVacancyClient>();
                MockRecruitVacancyClient = new Mock<IRecruitVacancyClient>();

                User = VacancyOrchestratorTestData.GetVacancyUser();
                Vacancy = VacancyOrchestratorTestData.GetPart1CompleteVacancy();
            }

            public WageOrchestratorTestsFixture WithWageType(WageType wageType)
            {
                Vacancy.Wage.WageType = wageType;
                return this;
            }

            public WageOrchestratorTestsFixture WithFixedWageYearlyAmount(decimal fixedWageYearlyAmmount)
            {
                Vacancy.Wage.FixedWageYearlyAmount = fixedWageYearlyAmmount;
                return this;
            }

            public WageOrchestratorTestsFixture WithWageAdditionalInformation(string wageAdditionalInformation)
            {
                Vacancy.Wage.WageAdditionalInformation = wageAdditionalInformation;
                return this;
            }

            public void Setup()
            {
                MockRecruitVacancyClient.Setup(x => x.GetVacancyAsync(Vacancy.Id)).ReturnsAsync(Vacancy);
                MockRecruitVacancyClient.Setup(x => x.Validate(Vacancy, ValidationRules)).Returns(new EntityValidationResult());
                MockRecruitVacancyClient.Setup(x => x.UpdateDraftVacancyAsync(It.IsAny<Vacancy>(), User));
                MockRecruitVacancyClient.Setup(x => x.UpdateEmployerProfileAsync(It.IsAny<EmployerProfile>(), User));

                Sut = new WageOrchestrator(MockClient.Object, MockRecruitVacancyClient.Object, Mock.Of<ILogger<WageOrchestrator>>(), 
                    Mock.Of<IReviewSummaryService>(), Mock.Of<IMinimumWageProvider>());
            }

            public async Task PostWageEditModelAsync(WageEditModel model)
            {
                await Sut.PostWageEditModelAsync(model, User);
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

            public Mock<IEmployerVacancyClient> MockClient { get; set; }
            public Mock<IRecruitVacancyClient> MockRecruitVacancyClient { get; set; }
        }
    }
}
