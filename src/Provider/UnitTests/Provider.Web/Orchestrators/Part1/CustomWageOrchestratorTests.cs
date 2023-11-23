using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.UnitTests.Provider.Web.HardMocks;
using Esfa.Recruit.Provider.Web;
using Esfa.Recruit.Provider.Web.Orchestrators.Part1;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.CustomWage;
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
    public class CustomWageOrchestratorTests
    {
        private CustomWageOrchestratorTestsFixture _fixture;

        public CustomWageOrchestratorTests()
        {
            _fixture = new CustomWageOrchestratorTestsFixture();
        }

        [Theory]
        [InlineData(11000, true)]
        [InlineData(10000, false)]
        public async Task WhenUpdated_ShouldFlagFieldIndicators(decimal fixedWageYearlyAmount, bool fieldIndicatorSet)
        {
            _fixture
                .WithWageType(WageType.FixedWage)
                .WithFixedWageYearlyAmount(10000)
                .Setup();

            var wageEditModel = new CustomWageEditModel
            {
                Ukprn = _fixture.Vacancy.TrainingProvider.Ukprn.Value,
                VacancyId = _fixture.Vacancy.Id,
                FixedWageYearlyAmount = fixedWageYearlyAmount.ToString()
            };

            await _fixture.PostCustomWageEditModelAsync(wageEditModel);

            _fixture.VerifyProviderReviewFieldIndicators(FieldIdentifiers.Wage, fieldIndicatorSet);
        }

        public class CustomWageOrchestratorTestsFixture
        {
            private const VacancyRuleSet ValidationRules = VacancyRuleSet.Wage | VacancyRuleSet.MinimumWage;
            public VacancyUser User { get; }
            public Vacancy Vacancy { get; }
            public CustomWageOrchestrator Sut {get; private set;}

            public CustomWageOrchestratorTestsFixture()
            {
                MockClient = new Mock<IProviderVacancyClient>();
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
                var utility = new Utility(MockRecruitVacancyClient.Object);
                
                Sut = new CustomWageOrchestrator(MockRecruitVacancyClient.Object, Mock.Of<ILogger<WageOrchestrator>>(), 
                    Mock.Of<IReviewSummaryService>(), Mock.Of<IMinimumWageProvider>(), utility);
            }

            public async Task PostCustomWageEditModelAsync(CustomWageEditModel model)
            {
                await Sut.PostCustomWageEditModelAsync(model, User);
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
