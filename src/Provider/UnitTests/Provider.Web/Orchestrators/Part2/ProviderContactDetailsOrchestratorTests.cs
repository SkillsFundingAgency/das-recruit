﻿using System.Linq;
using Esfa.Recruit.Provider.UnitTests.Provider.Web.HardMocks;
using Esfa.Recruit.Provider.Web;
using Esfa.Recruit.Provider.Web.Orchestrators.Part2;
using Esfa.Recruit.Provider.Web.ViewModels.Part2.ProviderContactDetails;
using Esfa.Recruit.Shared.Web.Domain;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Orchestrators.Part2;

public class ProviderContactDetailsOrchestratorTests
{
    private ProviderContactDetailsOrchestratorTestsFixture _fixture = new();
    
    [SetUp]
    public void Setup()
    {
        _fixture = new ProviderContactDetailsOrchestratorTestsFixture();
    }

    [TestCase("has a new value", "has a value", "has a value")]
    [TestCase("has a value", "has a new value", "has a value")]
    [TestCase("has a value", "has a value", "has a new value")]
    [TestCase("has a new value", "has a new value", "has a new value")]
    public async Task WhenProviderContactNameIsUpdated__ShouldCallUpdateDraftVacancy(string providerContactName, string providerContactEmail, string providerContactPhone)
    {
        _fixture
            .WithProviderContactName("has a value")
            .WithProviderContactEmail("has a value")
            .WithProviderContactPhone("has a value")
            .Setup();

        var providerContactDetailsEditModel = new ProviderContactDetailsEditModel
        {
            Ukprn = _fixture.Vacancy.TrainingProvider.Ukprn.Value,
            VacancyId = _fixture.Vacancy.Id,
            ProviderContactName = providerContactName,
            ProviderContactEmail = providerContactEmail,
            ProviderContactPhone = providerContactPhone
        };

        await _fixture.PostProviderContactDetailsEditModelAsync(providerContactDetailsEditModel);

        _fixture.VerifyUpdateDraftVacancyAsyncIsCalled();
    }

    [TestCase("has a new value", "has a value", "has a value")]
    [TestCase("has a value", "has a new value", "has a value")]
    [TestCase("has a value", "has a value", "has a new value")]
    [TestCase("has a new value", "has a new value", "has a new value")]
    public async Task WhenProviderContactNameIsUpdated_ShouldFlagProviderContactFieldIndicator(string providerContactName, string providerContactEmail, string providerContactPhone)
    {
        _fixture
            .WithProviderContactName("has a value")
            .WithProviderContactEmail("has a value")
            .WithProviderContactPhone("has a value")
            .Setup();

        var providerContactDetailsEditModel = new ProviderContactDetailsEditModel
        {
            Ukprn = _fixture.Vacancy.TrainingProvider.Ukprn.Value,
            VacancyId = _fixture.Vacancy.Id,
            ProviderContactName = providerContactName,
            ProviderContactEmail = providerContactEmail,
            ProviderContactPhone = providerContactPhone
        };

        await _fixture.PostProviderContactDetailsEditModelAsync(providerContactDetailsEditModel);

        _fixture.VerifyProviderReviewFieldIndicators(FieldIdentifiers.ProviderContact, true);
    }

    public class ProviderContactDetailsOrchestratorTestsFixture
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.ProviderContactDetails;
        public VacancyUser User { get; }
        public Vacancy Vacancy { get; }
        public ProviderContactDetailsOrchestrator Sut {get; private set;}

        public ProviderContactDetailsOrchestratorTestsFixture()
        {
            MockClient = new Mock<IProviderVacancyClient>();
            MockRecruitVacancyClient = new Mock<IRecruitVacancyClient>();

            User = VacancyOrchestratorTestData.GetVacancyUser();
            Vacancy = VacancyOrchestratorTestData.GetPart1CompleteVacancy();
        }

        public ProviderContactDetailsOrchestratorTestsFixture WithProviderContactName(string providerContactName)
        {
            if (Vacancy.ProviderContact == null)
                Vacancy.ProviderContact = new ContactDetail();

            Vacancy.ProviderContact.Name = providerContactName;
            return this;
        }

        public ProviderContactDetailsOrchestratorTestsFixture WithProviderContactEmail(string providerContactEmail)
        {
            if (Vacancy.ProviderContact == null)
                Vacancy.ProviderContact = new ContactDetail();

            Vacancy.ProviderContact.Email = providerContactEmail;
            return this;
        }

        public ProviderContactDetailsOrchestratorTestsFixture WithProviderContactPhone(string providerContactPhone)
        {
            if (Vacancy.ProviderContact == null)
                Vacancy.ProviderContact = new ContactDetail();

            Vacancy.ProviderContact.Phone = providerContactPhone;
            return this;
        }

        public void Setup()
        {
            MockRecruitVacancyClient.Setup(x => x.GetVacancyAsync(Vacancy.Id)).ReturnsAsync(Vacancy);
            MockRecruitVacancyClient.Setup(x => x.Validate(Vacancy, ValidationRules)).Returns(new EntityValidationResult());
            MockRecruitVacancyClient.Setup(x => x.UpdateDraftVacancyAsync(It.IsAny<Vacancy>(), User));
            MockRecruitVacancyClient.Setup(x => x.UpdateEmployerProfileAsync(It.IsAny<EmployerProfile>(), User));

            Sut = new ProviderContactDetailsOrchestrator(MockRecruitVacancyClient.Object, Mock.Of<ILogger<ProviderContactDetailsOrchestrator>>(), Mock.Of<IReviewSummaryService>(), new Utility(MockRecruitVacancyClient.Object, Mock.Of<ITaskListValidator>()));
        }

        public async Task PostProviderContactDetailsEditModelAsync(ProviderContactDetailsEditModel model)
        {
            await Sut.PostProviderContactDetailsEditModelAsync(model, User);
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
            MockRecruitVacancyClient.Verify(x => x.UpdateDraftVacancyAsync(It.Is<Vacancy>(c=>c.HasChosenProviderContactDetails.Value), User), Times.Once);
        }

        public Mock<IProviderVacancyClient> MockClient { get; set; }
        public Mock<IRecruitVacancyClient> MockRecruitVacancyClient { get; set; }
    }
}