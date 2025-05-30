﻿using System.Linq;
using Esfa.Recruit.Employer.UnitTests.Employer.Web.HardMocks;
using Esfa.Recruit.Employer.Web;
using Esfa.Recruit.Employer.Web.Orchestrators.Part1;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Wage;
using Esfa.Recruit.Shared.Web.Domain;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Orchestrators.Part1;

public class WageOrchestratorTests
{
    private WageOrchestratorTestsFixture _fixture;

    [SetUp]
    public void Setup()
    {
        _fixture = new WageOrchestratorTestsFixture();
    }

    [TestCase(WageType.FixedWage, 10000, "this is a value", true)]
    [TestCase(WageType.NationalMinimumWage, 10000, "this is a value", true)]
    [TestCase(WageType.NationalMinimumWageForApprentices, 11000, "this is a value", true)]
    [TestCase(WageType.CompetitiveSalary, 10000, "this is a new value", true)]
    public async Task WhenAdditionalInformationUpdated_ShouldFlagFieldIndicators(WageType wageType, decimal fixedWageYearlyAmmount, string wageAddtionalInformation, bool fieldIndicatorSet)
    {
        _fixture
            .WithWageType(wageType)
            .WithFixedWageYearlyAmount(fixedWageYearlyAmmount)
            .Setup();

        var wageExtraInformationViewModel = new WageExtraInformationViewModel
        {
            EmployerAccountId = _fixture.Vacancy.EmployerAccountId,
            VacancyId = _fixture.Vacancy.Id,
            WageType = wageType,
            FixedWageYearlyAmount = fixedWageYearlyAmmount.ToString(),
            WageAdditionalInformation = wageAddtionalInformation
        };

        await _fixture.PostExtraInformationEditModelAsync(wageExtraInformationViewModel);

        _fixture.VerifyEmployerReviewFieldIndicators(FieldIdentifiers.Wage, fieldIndicatorSet);
    }

    [TestCase(WageType.FixedWage, 10000, "this is a value", false)]
    [TestCase(WageType.NationalMinimumWage, 10000, "this is a value", true)]
    [TestCase(WageType.FixedWage, 11000, "this is a value", true)]
    [TestCase(WageType.FixedWage, 10000, "this is a new value", true)]
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
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.Wage;
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
            var utility = new Utility(MockRecruitVacancyClient.Object, Mock.Of<ITaskListValidator>());
                
            Sut = new WageOrchestrator(MockRecruitVacancyClient.Object, Mock.Of<ILogger<WageOrchestrator>>(), 
                Mock.Of<IReviewSummaryService>(), Mock.Of<IMinimumWageProvider>(), utility);
        }

        public async Task PostWageEditModelAsync(WageEditModel model)
        {
            await Sut.PostWageEditModelAsync(model, User);
        }

        public async Task PostExtraInformationEditModelAsync(WageExtraInformationViewModel model)
        {
            await Sut.PostExtraInformationEditModelAsync(model, User);
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
            Vacancy.EmployerReviewFieldIndicators.Single(p => p.FieldIdentifier == fieldIdentifier)
                .Should().NotBeNull().And
                .Match<EmployerReviewFieldIndicator>((x) => x.IsChangeRequested == value);
        }

        public Mock<IEmployerVacancyClient> MockClient { get; set; }
        public Mock<IRecruitVacancyClient> MockRecruitVacancyClient { get; set; }
    }
}