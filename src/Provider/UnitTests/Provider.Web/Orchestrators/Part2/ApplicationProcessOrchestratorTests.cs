﻿using System.Linq;
using Esfa.Recruit.Provider.UnitTests.Provider.Web.HardMocks;
using Esfa.Recruit.Provider.Web;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Orchestrators.Part2;
using Esfa.Recruit.Provider.Web.ViewModels.Part2.ApplicationProcess;
using Esfa.Recruit.Shared.Web.Domain;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Orchestrators.Part2;

public class ApplicationProcessOrchestratorTests
{
    private ApplicationProcessOrchestratorTestsFixture _fixture;

    [SetUp]
    public void Setup()
    {
        _fixture = new ApplicationProcessOrchestratorTestsFixture();
    }

    [Test]
    public async Task WhenApplicationMethodIsThroughFaaVacancy_ShouldCallUpdateDraftVacancy()
    {
        _fixture
            .WithApplicationMethod(ApplicationMethod.ThroughExternalApplicationSite)
            .WithApplicationInstructions("has a value")
            .WithApplicationUrl("has a value")
            .Setup();

        var applicationProcessEditModel = new ApplicationProcessEditModel
        {
            Ukprn = _fixture.Vacancy.TrainingProvider.Ukprn.Value,
            VacancyId = _fixture.Vacancy.Id,
            ApplicationMethod = ApplicationMethod.ThroughFindAnApprenticeship,
            ApplicationInstructions = "has a value",
        };

        await _fixture.PostApplicationProcessEditModelAsync(applicationProcessEditModel);

        _fixture.VerifyUpdateDraftVacancyAsyncIsCalled();
    }

    [Test]
    public async Task WhenApplicationMethodIsThroughFaaVacancy_ShouldOverwriteApplicationUrlAsNull()
    {
        _fixture
            .WithApplicationMethod(ApplicationMethod.ThroughExternalApplicationSite)
            .WithApplicationInstructions("has a value")
            .WithApplicationUrl("has a value")
            .Setup();

        var applicationProcessEditModel = new ApplicationProcessEditModel
        {
            Ukprn = _fixture.Vacancy.TrainingProvider.Ukprn.Value,
            VacancyId = _fixture.Vacancy.Id,
            ApplicationMethod = ApplicationMethod.ThroughFindAnApprenticeship,
            ApplicationUrl = "has a value"
        };

        await _fixture.PostApplicationProcessEditModelAsync(applicationProcessEditModel);

        _fixture.VerifyApplicationMethod(ApplicationMethod.ThroughFindAnApprenticeship);
        _fixture.VerifyOverwriteApplicationUrlAsNull();
    }

    [Test]
    public async Task WhenApplicationMethodIsThroughFaaVacancy_ShouldOverwriteApplicationInstructionsAsNull()
    {
        _fixture
            .WithApplicationMethod(ApplicationMethod.ThroughExternalApplicationSite)
            .WithApplicationInstructions("has a value")
            .WithApplicationUrl("has a value")
            .Setup();

        var applicationProcessEditModel = new ApplicationProcessEditModel
        {
            Ukprn = _fixture.Vacancy.TrainingProvider.Ukprn.Value,
            VacancyId = _fixture.Vacancy.Id,
            ApplicationMethod = ApplicationMethod.ThroughFindAnApprenticeship,
            ApplicationInstructions = "has a value",
        };

        await _fixture.PostApplicationProcessEditModelAsync(applicationProcessEditModel);

        _fixture.VerifyApplicationMethod(ApplicationMethod.ThroughFindAnApprenticeship);
        _fixture.VerifyOverwriteApplicationInstructionsAsNull();
    }

    [Test]
    public async Task WhenApplicationMethodIsThroughFaaVacancy_ShouldFlagAllApplicationFieldIndicators()
    {
        _fixture
            .WithApplicationMethod(ApplicationMethod.ThroughExternalApplicationSite)
            .WithApplicationInstructions("has a value")
            .WithApplicationUrl("has a value")
            .Setup();

        var applicationProcessEditModel = new ApplicationProcessEditModel
        {
            Ukprn = _fixture.Vacancy.TrainingProvider.Ukprn.Value,
            VacancyId = _fixture.Vacancy.Id,
            ApplicationMethod = ApplicationMethod.ThroughFindAnApprenticeship,
            ApplicationInstructions = "has a value",
            ApplicationUrl = "has a value"
        };

        await _fixture.PostApplicationProcessEditModelAsync(applicationProcessEditModel);

        _fixture.VerifyProviderReviewFieldIndicators(FieldIdentifiers.ApplicationMethod, true);
        _fixture.VerifyProviderReviewFieldIndicators(FieldIdentifiers.ApplicationInstructions, true);
        _fixture.VerifyProviderReviewFieldIndicators(FieldIdentifiers.ApplicationUrl, true);
    }

    [Test]
    public async Task WhenApplicationInstructionsIsUpdated_ShouldFlagApplicationInstructionsFieldIndicator()
    {
        _fixture
            .WithApplicationMethod(ApplicationMethod.ThroughExternalApplicationSite)
            .WithApplicationInstructions("has a value")
            .WithApplicationUrl("has a value")
            .Setup();

        var applicationProcessEditModel = new ApplicationProcessEditModel
        {
            Ukprn = _fixture.Vacancy.TrainingProvider.Ukprn.Value,
            VacancyId = _fixture.Vacancy.Id,
            ApplicationMethod = ApplicationMethod.ThroughExternalApplicationSite,
            ApplicationInstructions = "has a new value",
            ApplicationUrl = "has a value"
        };

        await _fixture.PostApplicationProcessEditModelAsync(applicationProcessEditModel);

        _fixture.VerifyProviderReviewFieldIndicators(FieldIdentifiers.ApplicationMethod, false);
        _fixture.VerifyProviderReviewFieldIndicators(FieldIdentifiers.ApplicationInstructions, true);
        _fixture.VerifyProviderReviewFieldIndicators(FieldIdentifiers.ApplicationUrl, false);
    }

    [Test]
    public async Task WhenApplicationUrlIsUpdated_ShouldFlagApplicationInstructionsFieldIndicator()
    {
        _fixture
            .WithApplicationMethod(ApplicationMethod.ThroughExternalApplicationSite)
            .WithApplicationInstructions("has a value")
            .WithApplicationUrl("has a value")
            .Setup();

        var applicationProcessEditModel = new ApplicationProcessEditModel
        {
            Ukprn = _fixture.Vacancy.TrainingProvider.Ukprn.Value,
            VacancyId = _fixture.Vacancy.Id,
            ApplicationMethod = ApplicationMethod.ThroughExternalApplicationSite,
            ApplicationInstructions = "has a value",
            ApplicationUrl = "has a new value"
        };

        await _fixture.PostApplicationProcessEditModelAsync(applicationProcessEditModel);

        _fixture.VerifyProviderReviewFieldIndicators(FieldIdentifiers.ApplicationMethod, false);
        _fixture.VerifyProviderReviewFieldIndicators(FieldIdentifiers.ApplicationInstructions, false);
        _fixture.VerifyProviderReviewFieldIndicators(FieldIdentifiers.ApplicationUrl, true);
    }

    public class ApplicationProcessOrchestratorTestsFixture
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.ApplicationMethod;
        public VacancyUser User { get; }
        public Vacancy Vacancy { get; }
        public ApplicationProcessOrchestrator Sut { get; private set; }

        public ApplicationProcessOrchestratorTestsFixture()
        {
            MockClient = new Mock<IProviderVacancyClient>();
            MockRecruitVacancyClient = new Mock<IRecruitVacancyClient>();
            User = VacancyOrchestratorTestData.GetVacancyUser();
            Vacancy = VacancyOrchestratorTestData.GetPart1CompleteVacancy();
        }

        public ApplicationProcessOrchestratorTestsFixture WithApplicationMethod(ApplicationMethod applicationMethod)
        {
            Vacancy.ApplicationMethod = applicationMethod;
            return this;
        }

        public ApplicationProcessOrchestratorTestsFixture WithApplicationInstructions(string applicationInstructions)
        {
            Vacancy.ApplicationInstructions = applicationInstructions;
            return this;
        }

        public ApplicationProcessOrchestratorTestsFixture WithApplicationUrl(string applicationUrl)
        {
            Vacancy.ApplicationUrl = applicationUrl;
            return this;
        }

        public void Setup()
        {
            MockRecruitVacancyClient.Setup(x => x.GetVacancyAsync(Vacancy.Id)).ReturnsAsync(Vacancy);
            MockRecruitVacancyClient.Setup(x => x.Validate(Vacancy, ValidationRules)).Returns(new EntityValidationResult());
            MockRecruitVacancyClient.Setup(x => x.UpdateDraftVacancyAsync(It.IsAny<Vacancy>(), User));
            MockRecruitVacancyClient.Setup(x => x.UpdateEmployerProfileAsync(It.IsAny<EmployerProfile>(), User));

            Sut = new ApplicationProcessOrchestrator(MockRecruitVacancyClient.Object, 
                Options.Create(new ExternalLinksConfiguration()), Mock.Of<ILogger<ApplicationProcessOrchestrator>>(), Mock.Of<IReviewSummaryService>(), new Utility(MockRecruitVacancyClient.Object, Mock.Of<ITaskListValidator>()));
        }

        public async Task PostApplicationProcessEditModelAsync(ApplicationProcessEditModel model)
        {
            await Sut.PostApplicationProcessEditModelAsync(model, User);
        }

        public void VerifyProviderReviewFieldIndicators(string fieldIdentifier, bool value)
        {
            Vacancy.ProviderReviewFieldIndicators.Single(p => p.FieldIdentifier == fieldIdentifier)
                .Should().NotBeNull().And
                .Match<ProviderReviewFieldIndicator>((x) => x.IsChangeRequested == value);
        }

        public void VerifyApplicationMethod(ApplicationMethod applicationMethod)
        {
            Vacancy.ApplicationMethod.HasValue.Should().BeTrue();
            Vacancy.ApplicationMethod.Value.Should().Be(applicationMethod);
        }

        public void VerifyOverwriteApplicationInstructionsAsNull()
        {
            Vacancy.ApplicationInstructions.Should().BeNull();
        }

        public void VerifyOverwriteApplicationUrlAsNull()
        {
            Vacancy.ApplicationUrl.Should().BeNull();
        }

        public void VerifyUpdateDraftVacancyAsyncIsCalled()
        {
            MockRecruitVacancyClient.Verify(x => x.UpdateDraftVacancyAsync(Vacancy, User), Times.Once);
        }

        public Mock<IProviderVacancyClient> MockClient { get; set; }
        public Mock<IRecruitVacancyClient> MockRecruitVacancyClient { get; set; }
    }
}