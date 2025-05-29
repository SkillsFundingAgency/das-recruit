using System.Linq;
using Esfa.Recruit.Provider.UnitTests.Provider.Web.HardMocks;
using Esfa.Recruit.Provider.Web;
using Esfa.Recruit.Provider.Web.Orchestrators.Part1;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.CustomWage;
using Esfa.Recruit.Shared.Web.Domain;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Orchestrators.Part1;

public class CustomWageOrchestratorTests
{
    [TestCase(11000, true)]
    [TestCase(10000, false)]
    public async Task WhenUpdated_ShouldFlagFieldIndicators(decimal fixedWageYearlyAmount, bool fieldIndicatorSet)
    {
        var fixture = new CustomWageOrchestratorTestsFixture();
        fixture
            .WithWageType(WageType.FixedWage)
            .WithFixedWageYearlyAmount(10000)
            .Setup();

        var wageEditModel = new CustomWageEditModel
        {
            Ukprn = fixture.Vacancy.TrainingProvider.Ukprn.Value,
            VacancyId = fixture.Vacancy.Id,
            FixedWageYearlyAmount = fixedWageYearlyAmount.ToString()
        };

        await fixture.PostCustomWageEditModelAsync(wageEditModel);

        fixture.VerifyProviderReviewFieldIndicators(FieldIdentifiers.Wage, fieldIndicatorSet);
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
            var utility = new Utility(MockRecruitVacancyClient.Object, Mock.Of<ITaskListValidator>());
                
            Sut = new CustomWageOrchestrator(MockRecruitVacancyClient.Object, Mock.Of<ILogger<WageOrchestrator>>(), 
                Mock.Of<IReviewSummaryService>(), Mock.Of<IMinimumWageProvider>(), utility);
        }

        public async Task PostCustomWageEditModelAsync(CustomWageEditModel model)
        {
            await Sut.PostCustomWageEditModelAsync(model, User);
        }

        public void VerifyProviderReviewFieldIndicators(string fieldIdentifier, bool value)
        {
            Vacancy.ProviderReviewFieldIndicators.Single(p => p.FieldIdentifier == fieldIdentifier)
                .Should().NotBeNull().And
                .Match<ProviderReviewFieldIndicator>((x) => x.IsChangeRequested == value);
        }

        public Mock<IProviderVacancyClient> MockClient { get; set; }
        public Mock<IRecruitVacancyClient> MockRecruitVacancyClient { get; set; }
    }
}