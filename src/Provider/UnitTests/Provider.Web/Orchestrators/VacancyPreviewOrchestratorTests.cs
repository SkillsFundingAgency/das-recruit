using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Mappings;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.ViewModels.VacancyPreview;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Rules.VacancyRules;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.PasAccount;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Orchestrators
{
    public class VacancyPreviewOrchestratorTests
    {
        [Theory]
        [InlineData(true, true, true)]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(false, false, false)]
        public async Task SubmitVacancyAsync_ShouldNotSubmitWhenMissingAgreements(
            bool hasLegalEntityAgreement, bool hasProviderAgreement, bool shouldBeSubmitted)
        {
            var vacancyId = Guid.NewGuid();
            const long ukprn = 12345678;
            const string employerAccountId = "ABCDEF";
            const long legalEntityId = 1234;

            var fixture = new Fixture();
            var vacancy = fixture.Create<Vacancy>();
            vacancy.Id = vacancyId;
            vacancy.TrainingProvider.Ukprn = ukprn;
            vacancy.Status = VacancyStatus.Draft;
            vacancy.IsDeleted = false;
            vacancy.EmployerAccountId = employerAccountId;
            vacancy.LegalEntityId = legalEntityId;
            vacancy.OwnerType = OwnerType.Provider;

            var client = new Mock<IProviderVacancyClient>();

            var vacancyClient = new Mock<IRecruitVacancyClient>();
            vacancyClient.Setup(c => c.GetVacancyAsync(vacancyId))
                .ReturnsAsync(vacancy);

            vacancyClient.Setup(c => c.GetEmployerNameAsync(vacancy))
                .ReturnsAsync("employer name from employer service");

            vacancyClient.Setup(c => c.Validate(vacancy, It.IsAny<Recruit.Vacancies.Client.Application.Validation.VacancyRuleSet>()))
                .Returns(new EntityValidationResult());

            var logger = new Mock<ILogger<VacancyPreviewOrchestrator>>();

            var geocodeImageService = new Mock<IGeocodeImageService>();
            var externalLinks = new Mock<IOptions<ExternalLinksConfiguration>>();
            var mapper = new DisplayVacancyViewModelMapper(geocodeImageService.Object, externalLinks.Object, vacancyClient.Object);

            var review = new Mock<IReviewSummaryService>();

            var legalEntityAgreement = new Mock<ILegalEntityAgreementService>();
            legalEntityAgreement.Setup(l => l.HasLegalEntityAgreementAsync(employerAccountId, legalEntityId))
                .ReturnsAsync(hasLegalEntityAgreement);

            var agreementServiceMock = new Mock<ITrainingProviderAgreementService>();
            agreementServiceMock.Setup(t => t.HasAgreementAsync(ukprn))
                .ReturnsAsync(hasProviderAgreement);

            var messagingMock = new Mock<IMessaging>();

            var orch = new VacancyPreviewOrchestrator(
                client.Object, vacancyClient.Object, logger.Object, mapper, review.Object, 
                legalEntityAgreement.Object, agreementServiceMock.Object, messagingMock.Object);

            var m = new SubmitEditModel
            {
                VacancyId = vacancyId,
                Ukprn = ukprn
            };

            var user = new VacancyUser();

            var actualResponse = await orch.SubmitVacancyAsync(m, user);

            var submittedTimes = shouldBeSubmitted ? Times.Once() : Times.Never();
            messagingMock.Verify(c => c.SendCommandAsync(It.IsAny<ICommand>()), submittedTimes);

            actualResponse.Data.IsSubmitted.Should().Be(shouldBeSubmitted);
            actualResponse.Data.HasLegalEntityAgreement.Should().Be(hasLegalEntityAgreement);
            actualResponse.Data.HasProviderAgreement.Should().Be(hasProviderAgreement);
        }
    }
}
