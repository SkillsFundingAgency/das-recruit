﻿using System.Linq;
using Esfa.Recruit.Provider.UnitTests.Provider.Web.HardMocks;
using Esfa.Recruit.Provider.Web;
using Esfa.Recruit.Provider.Web.Orchestrators.Part1;
using Esfa.Recruit.Provider.Web.ViewModels.Part2.ShortDescription;
using Esfa.Recruit.Shared.Web.Domain;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Orchestrators.Part2;

public class ShortDescriptionOrchestratorTests
{
    private ShortDescriptionOrchestratorTestsFixture _fixture;

    [SetUp]
    public void Setup()
    {
        _fixture = new ShortDescriptionOrchestratorTestsFixture();
    }

    [Test]
    public async Task WhenUpdated__ShouldCallUpdateDraftVacancy()
    {
        _fixture
            .WithShortDescription("has a value")
            .Setup();

        var shortDescriptionEditModel = new ShortDescriptionEditModel
        {
            Ukprn = _fixture.Vacancy.TrainingProvider.Ukprn.Value,
            VacancyId = _fixture.Vacancy.Id,
            ShortDescription = "has a new value"
        };

        await _fixture.PostShortDescriptionEditModelAsync(shortDescriptionEditModel);

        _fixture.VerifyUpdateDraftVacancyAsyncIsCalled();
    }

    [Test]
    public async Task WhenShortDescriptionIsUpdated_ShouldFlagThingsToConsiderFieldIndicator()
    {
        _fixture
            .WithShortDescription("has a value")
            .Setup();

        var shortDescriptionEditModel = new ShortDescriptionEditModel
        {
            Ukprn = _fixture.Vacancy.TrainingProvider.Ukprn.Value,
            VacancyId = _fixture.Vacancy.Id,
            ShortDescription = "has a new value"
        };

        await _fixture.PostShortDescriptionEditModelAsync(shortDescriptionEditModel);

        _fixture.VerifyProviderReviewFieldIndicators(FieldIdentifiers.ShortDescription, true);
    }

    public class ShortDescriptionOrchestratorTestsFixture
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.ShortDescription;
        public VacancyUser User { get; }
        public Vacancy Vacancy { get; }
        public ShortDescriptionOrchestrator Sut {get; private set;}

        public ShortDescriptionOrchestratorTestsFixture()
        {
            MockClient = new Mock<IProviderVacancyClient>();
            MockRecruitVacancyClient = new Mock<IRecruitVacancyClient>();

            User = VacancyOrchestratorTestData.GetVacancyUser();
            Vacancy = VacancyOrchestratorTestData.GetPart1CompleteVacancy();
        }

        public ShortDescriptionOrchestratorTestsFixture WithShortDescription(string shortDescription)
        {
            Vacancy.ShortDescription = shortDescription;
            return this;
        }

        public void Setup()
        {
            MockRecruitVacancyClient.Setup(x => x.GetVacancyAsync(Vacancy.Id)).ReturnsAsync(Vacancy);
            MockRecruitVacancyClient.Setup(x => x.Validate(Vacancy, ValidationRules)).Returns(new EntityValidationResult());
            MockRecruitVacancyClient.Setup(x => x.UpdateDraftVacancyAsync(It.IsAny<Vacancy>(), User));
            MockRecruitVacancyClient.Setup(x => x.UpdateEmployerProfileAsync(It.IsAny<EmployerProfile>(), User));

            Sut = new ShortDescriptionOrchestrator(MockRecruitVacancyClient.Object, Mock.Of<ILogger<ShortDescriptionOrchestrator>>(), Mock.Of<IReviewSummaryService>(), new Utility(MockRecruitVacancyClient.Object, Mock.Of<ITaskListValidator>()));
        }

        public async Task PostShortDescriptionEditModelAsync(ShortDescriptionEditModel model)
        {
            await Sut.PostShortDescriptionEditModelAsync(model, User);
        }

        public void VerifyProviderReviewFieldIndicators(string fieldIdentifier, bool value)
        {
            Vacancy.ProviderReviewFieldIndicators.FirstOrDefault(p => p.FieldIdentifier == fieldIdentifier)
                .Should().NotBeNull().And
                .Match<ProviderReviewFieldIndicator>((x) => x.IsChangeRequested == value);
        }

        public void VerifyUpdateDraftVacancyAsyncIsCalled()
        {
            MockRecruitVacancyClient.Verify(x => x.UpdateDraftVacancyAsync(Vacancy, User), Times.Once);
        }

        public Mock<IProviderVacancyClient> MockClient { get; set; }
        public Mock<IRecruitVacancyClient> MockRecruitVacancyClient { get; set; }
    }
}