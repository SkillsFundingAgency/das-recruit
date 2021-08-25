﻿using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.UnitTests.Provider.Web.HardMocks;
using Esfa.Recruit.Provider.Web.Orchestrators.Part2;
using Esfa.Recruit.Provider.Web.ViewModels.Part2.VacancyDescription;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Orchestrators.Part2
{
    public class VacancyDescriptionOrchestratorTests
    {
        private VacancyDescriptionOrchestratorTestsFixture _fixture;

        public VacancyDescriptionOrchestratorTests()
        {
            _fixture = new VacancyDescriptionOrchestratorTestsFixture();
        }

        [Theory]
        [InlineData("has a new value", "has a value", "has a value")]
        [InlineData("has a value", "has a new value", "has a value")]
        [InlineData("has a value", "has a value", "has a new value")]
        [InlineData("has a new value", "has a new value", "has a new value")]
        public async Task WhenUpdated__ShouldCallUpdateDraftVacancy(string description, string trainingDescription, string outcomeDescription)
        {
            _fixture
                .WithDescription("has a value")
                .WithTrainingDescription("has a value")
                .WithOutcomeDescription("has a value")
                .Setup();

            var vacancyDescriptionEditModel = new VacancyDescriptionEditModel
            {
                Ukprn = _fixture.Vacancy.TrainingProvider.Ukprn.Value,
                VacancyId = _fixture.Vacancy.Id,
                VacancyDescription = description,
                TrainingDescription = trainingDescription,
                OutcomeDescription = outcomeDescription
            };

            await _fixture.PostVacancyDescriptionEditModelAsync(vacancyDescriptionEditModel);

            _fixture.VerifyUpdateDraftVacancyAsyncIsCalled();
        }

        [Theory]
        [InlineData("has a new value", "has a value", "has a value", new string[] { FieldIdentifiers.VacancyDescription }, new string[] { FieldIdentifiers.TrainingDescription, FieldIdentifiers.OutcomeDescription })]
        [InlineData("has a value", "has a new value", "has a value", new string[] { FieldIdentifiers.TrainingDescription }, new string[] { FieldIdentifiers.VacancyDescription, FieldIdentifiers.OutcomeDescription })]
        [InlineData("has a value", "has a value", "has a new value", new string[] { FieldIdentifiers.OutcomeDescription }, new string[] { FieldIdentifiers.VacancyDescription, FieldIdentifiers.Training})]
        [InlineData("has a new value", "has a new value", "has a new value", new string[] { FieldIdentifiers.VacancyDescription, FieldIdentifiers.TrainingDescription , FieldIdentifiers.OutcomeDescription }, new string[] { })]
        public async Task WhenShortDescriptionIsUpdated_ShouldFlagFieldIndicators(string description, string trainingDescription, string outcomeDescription, string[] setFieldIndicators, string[] unsetFieldIndicators)
        {
            _fixture
                .WithDescription("has a value")
                .WithTrainingDescription("has a value")
                .WithOutcomeDescription("has a value")
                .Setup();

            var vacancyDescriptionEditModel = new VacancyDescriptionEditModel
            {
                Ukprn = _fixture.Vacancy.TrainingProvider.Ukprn.Value,
                VacancyId = _fixture.Vacancy.Id,
                VacancyDescription = description,
                TrainingDescription = trainingDescription,
                OutcomeDescription = outcomeDescription
            };

            await _fixture.PostVacancyDescriptionEditModelAsync(vacancyDescriptionEditModel);

            _fixture.VerifyProviderReviewFieldIndicators(setFieldIndicators, unsetFieldIndicators);
        }

        public class VacancyDescriptionOrchestratorTestsFixture
        {
            private const VacancyRuleSet ValidationRules = VacancyRuleSet.Description | VacancyRuleSet.TrainingDescription | VacancyRuleSet.OutcomeDescription;
            public VacancyUser User { get; }
            public Vacancy Vacancy { get; }
            public VacancyDescriptionOrchestrator Sut {get; private set;}

            public VacancyDescriptionOrchestratorTestsFixture()
            {
                MockClient = new Mock<IProviderVacancyClient>();
                MockRecruitVacancyClient = new Mock<IRecruitVacancyClient>();

                User = VacancyOrchestratorTestData.GetVacancyUser();
                Vacancy = VacancyOrchestratorTestData.GetPart1CompleteVacancy();
            }

            public VacancyDescriptionOrchestratorTestsFixture WithDescription(string description)
            {
                Vacancy.Description = description;
                return this;
            }

            public VacancyDescriptionOrchestratorTestsFixture WithTrainingDescription(string trainingDescription)
            {
                Vacancy.TrainingDescription = trainingDescription;
                return this;
            }

            public VacancyDescriptionOrchestratorTestsFixture WithOutcomeDescription(string outcomeDescription)
            {
                Vacancy.OutcomeDescription = outcomeDescription;
                return this;
            }

            public void Setup()
            {
                MockRecruitVacancyClient.Setup(x => x.GetVacancyAsync(Vacancy.Id)).ReturnsAsync(Vacancy);
                MockRecruitVacancyClient.Setup(x => x.Validate(Vacancy, ValidationRules)).Returns(new EntityValidationResult());
                MockRecruitVacancyClient.Setup(x => x.UpdateDraftVacancyAsync(It.IsAny<Vacancy>(), User));
                MockRecruitVacancyClient.Setup(x => x.UpdateEmployerProfileAsync(It.IsAny<EmployerProfile>(), User));

                Sut = new VacancyDescriptionOrchestrator(MockClient.Object, MockRecruitVacancyClient.Object, Mock.Of<ILogger<VacancyDescriptionOrchestrator>>(), Mock.Of<IReviewSummaryService>());
            }

            public async Task PostVacancyDescriptionEditModelAsync(VacancyDescriptionEditModel model)
            {
                await Sut.PostVacancyDescriptionEditModelAsync(model, User);
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

            public void VerifyUpdateDraftVacancyAsyncIsCalled()
            {
                MockRecruitVacancyClient.Verify(x => x.UpdateDraftVacancyAsync(Vacancy, User), Times.Once);
            }

            public Mock<IProviderVacancyClient> MockClient { get; set; }
            public Mock<IRecruitVacancyClient> MockRecruitVacancyClient { get; set; }
        }
    }
}
