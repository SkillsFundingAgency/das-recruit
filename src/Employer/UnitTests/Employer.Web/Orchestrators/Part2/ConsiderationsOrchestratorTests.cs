﻿using System.Linq;
using Esfa.Recruit.Employer.UnitTests.Employer.Web.HardMocks;
using Esfa.Recruit.Employer.Web;
using Esfa.Recruit.Employer.Web.Orchestrators.Part2;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Shared.Web.Domain;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Orchestrators.Part2;

public class ConsiderationsOrchestratorTests
{
    private ConsiderationsOrchestratorTestsFixture _fixture;

    [SetUp]
    public void Setup()
    {
        _fixture = new ConsiderationsOrchestratorTestsFixture();
    }

    [Test]
    public async Task WhenUpdated__ShouldCallUpdateDraftVacancy()
    {
        _fixture
            .WithThingsToConsider("has a value")
            .Setup();

        var thingsToConsiderEditModel = new ConsiderationsEditModel
        {
            EmployerAccountId = _fixture.Vacancy.EmployerAccountId,
            VacancyId = _fixture.Vacancy.Id,
            ThingsToConsider = "has a new value"
        };

        await _fixture.PostConsiderationsEditModelAsync(thingsToConsiderEditModel);

        _fixture.VerifyUpdateDraftVacancyAsyncIsCalled();
    }

    [Test]
    public async Task WhenThingsToConsiderIsUpdated_ShouldFlagThingsToConsiderFieldIndicator()
    {
        _fixture
            .WithThingsToConsider("has a value")
            .Setup();

        var thingsToConsiderEditModel = new ConsiderationsEditModel
        {
            EmployerAccountId = _fixture.Vacancy.EmployerAccountId,
            VacancyId = _fixture.Vacancy.Id,
            ThingsToConsider = "has a new value"
        };

        await _fixture.PostConsiderationsEditModelAsync(thingsToConsiderEditModel);

        _fixture.VerifyEmployerReviewFieldIndicators(FieldIdentifiers.ThingsToConsider, true);
    }

    public class ConsiderationsOrchestratorTestsFixture
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.ThingsToConsider;
        public VacancyUser User { get; }
        public Vacancy Vacancy { get; }
        public ConsiderationsOrchestrator Sut {get; private set;}

        public ConsiderationsOrchestratorTestsFixture()
        {
            MockClient = new Mock<IEmployerVacancyClient>();
            MockRecruitVacancyClient = new Mock<IRecruitVacancyClient>();

            User = VacancyOrchestratorTestData.GetVacancyUser();
            Vacancy = VacancyOrchestratorTestData.GetPart1CompleteVacancy();
        }

        public ConsiderationsOrchestratorTestsFixture WithThingsToConsider(string thingsToConsider)
        {
            Vacancy.ThingsToConsider = thingsToConsider;
            return this;
        }

        public void Setup()
        {
            MockRecruitVacancyClient.Setup(x => x.GetVacancyAsync(Vacancy.Id)).ReturnsAsync(Vacancy);
            MockRecruitVacancyClient.Setup(x => x.Validate(Vacancy, ValidationRules)).Returns(new EntityValidationResult());
            MockRecruitVacancyClient.Setup(x => x.UpdateDraftVacancyAsync(It.IsAny<Vacancy>(), User));
            MockRecruitVacancyClient.Setup(x => x.UpdateEmployerProfileAsync(It.IsAny<EmployerProfile>(), User));
            var utility = new Utility(MockRecruitVacancyClient.Object, Mock.Of<ITaskListValidator>());
                
            Sut = new ConsiderationsOrchestrator(Mock.Of<ILogger<ConsiderationsOrchestrator>>(), MockRecruitVacancyClient.Object, Mock.Of<IReviewSummaryService>(), utility);
        }

        public async Task PostConsiderationsEditModelAsync(ConsiderationsEditModel model)
        {
            await Sut.PostConsiderationsEditModelAsync(model, User);
        }

        public void VerifyEmployerReviewFieldIndicators(string fieldIdentifier, bool value)
        {
            Vacancy.EmployerReviewFieldIndicators.FirstOrDefault(p => p.FieldIdentifier == fieldIdentifier)
                .Should().NotBeNull().And
                .Match<EmployerReviewFieldIndicator>((x) => x.IsChangeRequested == value);
        }

        public void VerifyUpdateDraftVacancyAsyncIsCalled()
        {
            MockRecruitVacancyClient.Verify(x => x.UpdateDraftVacancyAsync(Vacancy, User), Times.Once);
        }

        public Mock<IEmployerVacancyClient> MockClient { get; set; }
        public Mock<IRecruitVacancyClient> MockRecruitVacancyClient { get; set; }
    }
}