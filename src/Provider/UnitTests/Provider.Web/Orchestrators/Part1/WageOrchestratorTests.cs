using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.UnitTests.Provider.Web.HardMocks;
using Esfa.Recruit.Provider.Web;
using Esfa.Recruit.Provider.Web.Orchestrators.Part1;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.Wage;
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

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Orchestrators.Part1
{
    public class WageOrchestratorTests
    {
        private WageOrchestratorTestsFixture _fixture;

        public WageOrchestratorTests()
        {
            _fixture = new WageOrchestratorTestsFixture();
        }

        [Fact]
        public async Task WhenCompetitiveSalaryUpdated_ShouldFlagFieldIndicators()
        {
            _fixture
                .Setup();

            _fixture.SetCompetitiveValidationRule();

            var wageExtraInformationViewModel = new CompetitiveWageEditModel()
            {
                Ukprn = _fixture.Vacancy.TrainingProvider.Ukprn.Value,
                VacancyId = _fixture.Vacancy.Id,
                WageType = WageType.CompetitiveSalary
            };

            await _fixture.PostExtraInformationEditModelAsync(wageExtraInformationViewModel);

            _fixture.VerifyProviderReviewFieldIndicators(FieldIdentifiers.Wage, true);
        }

        [Theory]
        [InlineData(WageType.FixedWage, "this is a value", true)]
        [InlineData(WageType.NationalMinimumWage, "this is a value", true)]
        [InlineData(WageType.NationalMinimumWageForApprentices, "this is a value", true)]
        [InlineData(WageType.CompetitiveSalary, "this is a new value", true)]
        public async Task WhenAdditionalInformationUpdated_ShouldFlagFieldIndicators(WageType wageType, string wageAddtionalInformation, bool fieldIndicatorSet)
        {
            _fixture
                .WithWageType(wageType)
                .Setup();

            var wageExtraInformationViewModel = new WageExtraInformationViewModel
            {
                Ukprn = _fixture.Vacancy.TrainingProvider.Ukprn.Value,
                VacancyId = _fixture.Vacancy.Id,
                WageType = wageType,
                WageAdditionalInformation = wageAddtionalInformation
            };

            await _fixture.PostExtraInformationEditModelAsync(wageExtraInformationViewModel);

            _fixture.VerifyProviderReviewFieldIndicators(FieldIdentifiers.Wage, fieldIndicatorSet);
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
                Ukprn = _fixture.Vacancy.TrainingProvider.Ukprn.Value,
                VacancyId = _fixture.Vacancy.Id,
                WageType = wageType,
                FixedWageYearlyAmount = fixedWageYearlyAmmount.ToString(),
                WageAdditionalInformation = wageAddtionalInformation
            };

            await _fixture.PostWageEditModelAsync(wageEditModel);

            _fixture.VerifyProviderReviewFieldIndicators(FieldIdentifiers.Wage, fieldIndicatorSet);
        }

        public class WageOrchestratorTestsFixture
        {
            private const VacancyRuleSet CompetitiveValidationRules = VacancyRuleSet.CompetitiveWage;
            private const VacancyRuleSet ValidationRules = VacancyRuleSet.Wage;

            public VacancyUser User { get; }
            public Vacancy Vacancy { get; }
            public WageOrchestrator Sut { get; private set; }

            public WageOrchestratorTestsFixture()
            {
                MockClient = new Mock<IProviderVacancyClient>();
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

                Sut = new WageOrchestrator(MockRecruitVacancyClient.Object, Mock.Of<ILogger<WageOrchestrator>>(),
                    Mock.Of<IReviewSummaryService>(), Mock.Of<IMinimumWageProvider>(), new Utility(MockRecruitVacancyClient.Object));
            }

            public async Task PostExtraInformationEditModelAsync(WageExtraInformationViewModel model)
            {
                await Sut.PostExtraInformationEditModelAsync(model, User);
            }
          
            public void SetCompetitiveValidationRule()
            {
                MockRecruitVacancyClient.Setup(x => x.Validate(Vacancy, CompetitiveValidationRules)).Returns(new EntityValidationResult()); ;
            }

            public async Task PostWageEditModelAsync(WageEditModel model)
            {
                await Sut.PostWageEditModelAsync(model, User);
            }

            public async Task PostExtraInformationEditModelAsync(CompetitiveWageEditModel model)
            {
                await Sut.PostCompetitiveWageEditModelAsync(model, User);
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

            public Mock<IProviderVacancyClient> MockClient { get; set; }
            public Mock<IRecruitVacancyClient> MockRecruitVacancyClient { get; set; }
        }
    }
}
