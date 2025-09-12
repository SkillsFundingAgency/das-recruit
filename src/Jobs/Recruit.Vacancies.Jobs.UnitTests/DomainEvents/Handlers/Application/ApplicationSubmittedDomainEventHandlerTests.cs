using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Communications;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;
using Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Application;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Xunit;
using Communication.Types;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;
using Microsoft.Azure.WebJobs;

namespace Recruit.Vacancies.Jobs.UnitTests.DomainEvents.Handlers
{
    public class ApplicationSubmittedDomainEventHandlerTests
    {
        private const long VacancyReference = 99999999;
        private readonly Guid _vacancyId = Guid.NewGuid();

        [Fact]
        public async Task GivenFeatureDisabled_CommunicationRequestIsSent_And_ApplicationReviewCreated()
        {
            // Arrange
            var application = new Esfa.Recruit.Vacancies.Client.Domain.Entities.Application
            {
                CandidateId = Guid.NewGuid(),
                VacancyReference = VacancyReference,
                ApplicationId = Guid.NewGuid()
            };

            var sourceEvent = new ApplicationSubmittedEvent
            {
                Application = application,
                VacancyId = _vacancyId
            };

            var eventPayload = JsonConvert.SerializeObject(sourceEvent);

            var mockCommQueue = new Mock<ICommunicationQueueService>();
            CommunicationRequest sentRequest = null;
            mockCommQueue.Setup(x => x.AddMessageAsync(It.IsAny<CommunicationRequest>()))
                .Returns(Task.CompletedTask)
                .Callback<CommunicationRequest>(cr => sentRequest = cr);

            var mockJobsClient = new Mock<Esfa.Recruit.Vacancies.Client.Infrastructure.Client.IJobsVacancyClient>();
            mockJobsClient.Setup(x => x.CreateApplicationReviewAsync(It.IsAny<Esfa.Recruit.Vacancies.Client.Domain.Entities.Application>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var mockOuterApiClient = new Mock<IOuterApiClient>();

            var mockFeature = new Mock<Esfa.Recruit.Vacancies.Client.Application.FeatureToggle.IFeature>();
            mockFeature.Setup(f => f.IsFeatureEnabled(It.Is<string>(s => s == FeatureNames.NotificationsMigration))).Returns(false);

            var sut = new ApplicationSubmittedDomainEventHandler(
                Mock.Of<ILogger<ApplicationSubmittedDomainEventHandler>>(),
                mockJobsClient.Object,
                mockCommQueue.Object,
                mockOuterApiClient.Object,
                mockFeature.Object);

            // Act
            await sut.HandleAsync(eventPayload);

            // Assert
            mockCommQueue.Verify(x => x.AddMessageAsync(It.IsAny<CommunicationRequest>()), Times.Once);
            sentRequest.Should().NotBeNull();
            sentRequest.Entities.Should().Contain(e => e.EntityType.Equals(CommunicationConstants.EntityTypes.Vacancy) && (long)e.EntityId == VacancyReference);
            mockJobsClient.Verify(x => x.CreateApplicationReviewAsync(It.IsAny<Esfa.Recruit.Vacancies.Client.Domain.Entities.Application>()), Times.Once);
        }

        [Fact]
        public async Task GivenFeatureEnabled_MockOuterApiClientIsCalledAndApplicationReviewCreated()
        {
            // Arrange
            var application = new Esfa.Recruit.Vacancies.Client.Domain.Entities.Application
            {
                CandidateId = Guid.NewGuid(),
                VacancyReference = VacancyReference,
                ApplicationId = Guid.NewGuid()
            };

            var sourceEvent = new ApplicationSubmittedEvent
            {
                Application = application,
                VacancyId = _vacancyId
            };

            var eventPayload = JsonConvert.SerializeObject(sourceEvent);

            var mockOuterApiClient = new Mock<IOuterApiClient>();
            mockOuterApiClient.Setup(x => x.Post(It.IsAny<PostApplicationSubmittedEventRequest>(), true))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var mockCommQueue = new Mock<ICommunicationQueueService>();
            // Should not be called when feature is enabled
            mockCommQueue.Setup(x => x.AddMessageAsync(It.IsAny<CommunicationRequest>())).Returns(Task.CompletedTask);

            var mockJobsClient = new Mock<Esfa.Recruit.Vacancies.Client.Infrastructure.Client.IJobsVacancyClient>();
            mockJobsClient.Setup(x => x.CreateApplicationReviewAsync(It.IsAny<Esfa.Recruit.Vacancies.Client.Domain.Entities.Application>())).Returns(Task.CompletedTask).Verifiable();

            var mockFeature = new Mock<Esfa.Recruit.Vacancies.Client.Application.FeatureToggle.IFeature>();
            mockFeature.Setup(f => f.IsFeatureEnabled(It.Is<string>(s => s == FeatureNames.NotificationsMigration))).Returns(true);

            var sut = new ApplicationSubmittedDomainEventHandler(
                Mock.Of<ILogger<ApplicationSubmittedDomainEventHandler>>(),
                mockJobsClient.Object,
                mockCommQueue.Object,
                mockOuterApiClient.Object,
                mockFeature.Object);

            // Act
            await sut.HandleAsync(eventPayload);

            // Assert
            mockOuterApiClient.Verify(x => x.Post(It.IsAny<PostApplicationSubmittedEventRequest>(), true), Times.Once);
            mockJobsClient.Verify(x => x.CreateApplicationReviewAsync(It.IsAny<Esfa.Recruit.Vacancies.Client.Domain.Entities.Application>()), Times.Once);
            mockCommQueue.Verify(x => x.AddMessageAsync(It.IsAny<CommunicationRequest>()), Times.Never);
        }
    }
}
