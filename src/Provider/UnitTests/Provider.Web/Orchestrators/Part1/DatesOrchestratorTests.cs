﻿using System.Globalization;
using System.Linq;
using Esfa.Recruit.Provider.UnitTests.Provider.Web.HardMocks;
using Esfa.Recruit.Provider.Web;
using Esfa.Recruit.Provider.Web.Orchestrators.Part1;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.Dates;
using Esfa.Recruit.Shared.Web.Domain;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Orchestrators.Part1;

public class DatesOrchestratorTests
{
    [TestCase("31/12/2021", "01/01/2001", false, new string[] { FieldIdentifiers.ClosingDate }, new string[] { FieldIdentifiers.PossibleStartDate, FieldIdentifiers.DisabilityConfident })]
    [TestCase("01/01/2001", "31/12/2021", false, new string[] { FieldIdentifiers.PossibleStartDate }, new string[] { FieldIdentifiers.ClosingDate, FieldIdentifiers.DisabilityConfident })]
    [TestCase("01/01/2001", "01/01/2001", true, new string[] { FieldIdentifiers.DisabilityConfident }, new string[] { FieldIdentifiers.ClosingDate, FieldIdentifiers.PossibleStartDate })]
    [TestCase("01/01/2001", "01/01/2001", false, new string[] { }, new string[] { FieldIdentifiers.ClosingDate, FieldIdentifiers.PossibleStartDate, FieldIdentifiers.DisabilityConfident })]
    [TestCase("31/12/2021", "31/12/2021", true, new string[] { FieldIdentifiers.ClosingDate, FieldIdentifiers.PossibleStartDate, FieldIdentifiers.DisabilityConfident }, new string[] { })]
    public async Task WhenUpdated_ShouldFlagFieldIndicators(string closingDate, string startDate, bool isDisablityConfident, string[] setFieldIdentifers, string[] unsetFieldIdentifiers)
    {
        DatesOrchestratorTestsFixture fixture = new();
        fixture
            .WithClosingDate("01/01/2001")
            .WithStartDate("01/01/2001")
            .WithDisabilityConfident(false)
            .Setup();

        var closingDateTime = DateTime.ParseExact(closingDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
        var startDateTime = DateTime.ParseExact(startDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);

        var datesEditModel = new DatesEditModel
        {
            Ukprn = fixture.Vacancy.TrainingProvider.Ukprn.Value,
            VacancyId = fixture.Vacancy.Id,
            ClosingDay = closingDateTime.Day.ToString(),
            ClosingMonth = closingDateTime.Month.ToString(),
            ClosingYear = closingDateTime.Year.ToString(),
            StartDay = startDateTime.Day.ToString(),
            StartMonth = startDateTime.Month.ToString(),
            StartYear = startDateTime.Year.ToString(),
            IsDisabilityConfident = isDisablityConfident
        };

        await fixture.PostDatesEditModelAsync(datesEditModel);

        fixture.VerifyEmployerReviewFieldIndicators(setFieldIdentifers, unsetFieldIdentifiers);
    }

    public class DatesOrchestratorTestsFixture
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.ClosingDate | VacancyRuleSet.StartDate | VacancyRuleSet.StartDateEndDate | VacancyRuleSet.TrainingExpiryDate;
        public VacancyUser User { get; }
        public Vacancy Vacancy { get; }
        public DatesOrchestrator Sut { get; private set; }

        public DatesOrchestratorTestsFixture()
        {
            MockClient = new Mock<IProviderVacancyClient>();
            MockRecruitVacancyClient = new Mock<IRecruitVacancyClient>();

            User = VacancyOrchestratorTestData.GetVacancyUser();
            Vacancy = VacancyOrchestratorTestData.GetPart1CompleteVacancy();
        }

        public DatesOrchestratorTestsFixture WithClosingDate(string closingDate)
        {
            Vacancy.ClosingDate = closingDate.AsDateTimeUk()?.ToUniversalTime();
            return this;
        }

        public DatesOrchestratorTestsFixture WithStartDate(string startDate)
        {
            Vacancy.StartDate = startDate.AsDateTimeUk()?.ToUniversalTime();
            return this;
        }

        public DatesOrchestratorTestsFixture WithDisabilityConfident(bool isDisablityConfident)
        {
            Vacancy.DisabilityConfident = isDisablityConfident ? DisabilityConfident.Yes : DisabilityConfident.No;
            return this;
        }

        public void Setup()
        {
            MockRecruitVacancyClient.Setup(x => x.GetVacancyAsync(Vacancy.Id)).ReturnsAsync(Vacancy);
            MockRecruitVacancyClient.Setup(x => x.Validate(Vacancy, ValidationRules)).Returns(new EntityValidationResult());
            MockRecruitVacancyClient.Setup(x => x.UpdateDraftVacancyAsync(It.IsAny<Vacancy>(), User));
            MockRecruitVacancyClient.Setup(x => x.UpdateEmployerProfileAsync(It.IsAny<EmployerProfile>(), User));

            Sut = new DatesOrchestrator(MockRecruitVacancyClient.Object, Mock.Of<ILogger<DatesOrchestrator>>(),
                Mock.Of<ITimeProvider>(), Mock.Of<IReviewSummaryService>(), Mock.Of<IApprenticeshipProgrammeProvider>(), new Utility(MockRecruitVacancyClient.Object, Mock.Of<ITaskListValidator>()));
        }

        public async Task PostDatesEditModelAsync(DatesEditModel model)
        {
            await Sut.PostDatesEditModelAsync(model, User);
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
            Vacancy.ProviderReviewFieldIndicators.Single(p => p.FieldIdentifier == fieldIdentifier)
                .Should().NotBeNull().And
                .Match<ProviderReviewFieldIndicator>((x) => x.IsChangeRequested == value);
        }

        public Mock<IProviderVacancyClient> MockClient { get; set; }
        public Mock<IRecruitVacancyClient> MockRecruitVacancyClient { get; set; }
    }
}