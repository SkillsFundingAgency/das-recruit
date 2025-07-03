﻿using System.Linq;
using Esfa.Recruit.Employer.UnitTests.Employer.Web.HardMocks;
using Esfa.Recruit.Employer.Web;
using Esfa.Recruit.Employer.Web.Orchestrators.Part2;
using Esfa.Recruit.Employer.Web.ViewModels.Part2.AboutEmployer;
using Esfa.Recruit.Shared.Web.Domain;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Orchestrators.Part2;

public class AboutEmployerOrchestratorTests
{
    private AboutEmployerOrchestratorTestsFixture _fixture;

    [SetUp]
    public void Setup()
    {
        _fixture = new AboutEmployerOrchestratorTestsFixture();
    }

    [TestCase("has a new value", "has a value", false, new string[] { FieldIdentifiers.EmployerDescription }, new string[] { FieldIdentifiers.EmployerWebsiteUrl, FieldIdentifiers.DisabilityConfident})]
    [TestCase("has a value", "has a new value", false, new string[] { FieldIdentifiers.EmployerWebsiteUrl }, new string[] { FieldIdentifiers.EmployerDescription, FieldIdentifiers.DisabilityConfident })]
    [TestCase("has a value", "has a value", true, new string[] { FieldIdentifiers.DisabilityConfident }, new string[] { FieldIdentifiers.EmployerDescription, FieldIdentifiers.EmployerWebsiteUrl })]
    [TestCase("has a value", "has a value", false, new string[] {  }, new string[] { FieldIdentifiers.EmployerDescription, FieldIdentifiers.EmployerWebsiteUrl, FieldIdentifiers.DisabilityConfident })]
    [TestCase("has a new value", "has a new value", true, new string[] { FieldIdentifiers.EmployerDescription, FieldIdentifiers.EmployerWebsiteUrl, FieldIdentifiers.DisabilityConfident }, new string[] {  })]
    public async Task WhenUpdated_ShouldFlagFieldIndicators(string employerDescription, string employerWebSiteUrl, bool isDisabilityConfident, string[] setFieldIdentifers, string [] unsetFieldIdentifiers)
    {
        _fixture
            .WithEmployerDescription("has a value")
            .WithEmployerWebsiteUrl("has a value")
            .WithDisabilityConfident(DisabilityConfident.No)
            .Setup();

        var aboutEmployerEditModel = new AboutEmployerEditModel
        {
            EmployerAccountId = _fixture.Vacancy.EmployerAccountId,
            VacancyId = _fixture.Vacancy.Id,
            EmployerDescription = employerDescription,
            EmployerWebsiteUrl = employerWebSiteUrl,
            IsDisabilityConfident = isDisabilityConfident
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
            MockClient = new Mock<IEmployerVacancyClient>();
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
            
        public AboutEmployerOrchestratorTestsFixture WithDisabilityConfident(DisabilityConfident disabilityConfident)
        {
            Vacancy.DisabilityConfident = disabilityConfident;
            return this;
        }

        public void Setup()
        {
            MockRecruitVacancyClient.Setup(x => x.GetVacancyAsync(Vacancy.Id)).ReturnsAsync(Vacancy);
            MockRecruitVacancyClient.Setup(x => x.GetEmployerProfileAsync(Vacancy.EmployerAccountId, Vacancy.AccountLegalEntityPublicHashedId)).ReturnsAsync(EmployerProfile);
            MockRecruitVacancyClient.Setup(x => x.Validate(Vacancy, ValidationRules)).Returns(new EntityValidationResult());
            MockRecruitVacancyClient.Setup(x => x.UpdateDraftVacancyAsync(It.IsAny<Vacancy>(), User));
            MockRecruitVacancyClient.Setup(x => x.UpdateEmployerProfileAsync(It.IsAny<EmployerProfile>(), User));

            var utility = new Utility(MockRecruitVacancyClient.Object, Mock.Of<ITaskListValidator>());
                
            Sut = new AboutEmployerOrchestrator(MockRecruitVacancyClient.Object, Mock.Of<ILogger<AboutEmployerOrchestrator>>(), Mock.Of<IReviewSummaryService>(), utility);
        }

        public async Task PostAboutEmployerEditModelAsync(AboutEmployerEditModel model)
        {
            await Sut.PostAboutEmployerEditModelAsync(model, User);
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
                .Single(p => p.FieldIdentifier == fieldIdentifier)
                .Should().NotBeNull().And
                .Match<EmployerReviewFieldIndicator>((x) => x.IsChangeRequested == value);
        }

        public Mock<IEmployerVacancyClient> MockClient { get; set; }
        public Mock<IRecruitVacancyClient> MockRecruitVacancyClient { get; set; }
    }
}