﻿using System.Linq;
using Esfa.Recruit.Employer.UnitTests.Employer.Web.HardMocks;
using Esfa.Recruit.Employer.Web;
using Esfa.Recruit.Employer.Web.Orchestrators.Part1;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.NumberOfPositions;
using Esfa.Recruit.Shared.Web.Domain;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Orchestrators.Part1;

public class NumberofPositionsOrchestratorTests
{
    private readonly NumberofPositionsOrchestratorTestsFixture _fixture = new();

    [TestCase(1, false)]
    [TestCase(2, true)]
    public async Task WhenUpdated_ShouldFlagFieldIndicators(int numberOfPositions, bool fieldIndicatorSet)
    {
        _fixture
            .WithNumberOfPositions(1)
            .Setup();

        var numberOfPositionsEditModel = new NumberOfPositionsEditModel
        {
            EmployerAccountId = _fixture.Vacancy.EmployerAccountId,
            VacancyId = _fixture.Vacancy.Id,
            NumberOfPositions = numberOfPositions.ToString()
        };

        await _fixture.PostNumberOfPositionsEditModelAsync(numberOfPositionsEditModel);

        _fixture.VerifyEmployerReviewFieldIndicators(FieldIdentifiers.NumberOfPositions, fieldIndicatorSet);
    }

    public class NumberofPositionsOrchestratorTestsFixture
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.NumberOfPositions;
        public VacancyUser User { get; }
        public Vacancy Vacancy { get; }
        public NumberOfPositionsOrchestrator Sut {get; private set;}

        public NumberofPositionsOrchestratorTestsFixture()
        {
            MockClient = new Mock<IEmployerVacancyClient>();
            MockRecruitVacancyClient = new Mock<IRecruitVacancyClient>();

            User = VacancyOrchestratorTestData.GetVacancyUser();
            Vacancy = VacancyOrchestratorTestData.GetPart1CompleteVacancy();
        }

        public NumberofPositionsOrchestratorTestsFixture WithNumberOfPositions(int numberOfPositions)
        {
            Vacancy.NumberOfPositions = numberOfPositions;
            return this;
        }

        public void Setup()
        {
            MockRecruitVacancyClient.Setup(x => x.GetVacancyAsync(Vacancy.Id)).ReturnsAsync(Vacancy);
            MockRecruitVacancyClient.Setup(x => x.Validate(Vacancy, ValidationRules)).Returns(new EntityValidationResult());
            MockRecruitVacancyClient.Setup(x => x.UpdateDraftVacancyAsync(It.IsAny<Vacancy>(), User));
            MockRecruitVacancyClient.Setup(x => x.UpdateEmployerProfileAsync(It.IsAny<EmployerProfile>(), User));
            var utility = new Utility(MockRecruitVacancyClient.Object, Mock.Of<ITaskListValidator>());
            Sut = new NumberOfPositionsOrchestrator(MockRecruitVacancyClient.Object, Mock.Of<ILogger<NumberOfPositionsOrchestrator>>(), 
                Mock.Of<IReviewSummaryService>(), utility);
        }

        public async Task PostNumberOfPositionsEditModelAsync(NumberOfPositionsEditModel model)
        {
            await Sut.PostNumberOfPositionsEditModelAsync(model, User);
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