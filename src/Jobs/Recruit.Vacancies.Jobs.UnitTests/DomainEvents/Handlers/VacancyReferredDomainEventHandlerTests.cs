using System;
using System.Linq;
using System.Threading.Tasks;
using Communication.Types;
using Esfa.Recruit.Vacancies.Client.Application.Communications;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;
using Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Vacancy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Xunit;

namespace Esfa.Recruit.Vacancies.Jobs.UnitTests.DomainEvents.Handlers
{
    public class VacancyReferredDomainEventHandlerTests
    {
        private const long ReferredVacancyReferenceNumber = 11111111;
        private Guid _exampleVacancyId = Guid.NewGuid();
        private VacancyReferredDomainEventHandler _sut;
        private Mock<ICommunicationQueueService> _mockCommunicationQueueService;
        private CommunicationRequest _sentCommRequest;
        private Mock<IFeature> _feature;

        [SetUp]
        public void SetUp()
        {
            _mockCommunicationQueueService = new Mock<ICommunicationQueueService>();
            _mockCommunicationQueueService.Setup(x => x.AddMessageAsync(It.IsAny<CommunicationRequest>()))
                            .Returns(Task.CompletedTask)
                            .Callback<CommunicationRequest>(cr => _sentCommRequest = cr);
            _feature = new Mock<IFeature>();
            _feature.Setup(x => x.IsFeatureEnabled(FeatureNames.NotificationsMigration)).Returns(false);

            _sut = new VacancyReferredDomainEventHandler(Mock.Of<ILogger<VacancyReferredDomainEventHandler>>(),
                                                        _mockCommunicationQueueService.Object, _feature.Object);
        }

        [Test]
        public async Task GivenVacancyReviewReferredVacancyEvent_VerifyCommunicationRequestIsSentToCommunicationQueue()
        {
            var sourceEvent = new VacancyReferredEvent
            {
                VacancyReference = ReferredVacancyReferenceNumber,
                VacancyId = _exampleVacancyId
            };
            var @event = JsonConvert.SerializeObject(sourceEvent);

            await _sut.HandleAsync(@event);

            _mockCommunicationQueueService.Verify(x => x.AddMessageAsync(It.IsAny<CommunicationRequest>()), Times.Once);
        }

        [Test]
        public async Task GivenVacancyReviewReferredVacancyEvent_VerifyCommunicationRequestHasExpectedData()
        {
            var sourceEvent = new VacancyReferredEvent
            {
                VacancyReference = ReferredVacancyReferenceNumber,
                VacancyId = _exampleVacancyId
            };
            var @event = JsonConvert.SerializeObject(sourceEvent);

            await _sut.HandleAsync(@event);

            _sentCommRequest
                .Entities
                .Any(cr => cr.EntityType.Equals(CommunicationConstants.EntityTypes.Vacancy) && ((long)cr.EntityId == ReferredVacancyReferenceNumber))
                .Should()
                .BeTrue();
        }

        [Test]
        public async Task When_Notification_Feature_Is_Enabled_Email_Is_Not_Sent()
        {
            _feature.Setup(x => x.IsFeatureEnabled(FeatureNames.NotificationsMigration)).Returns(true);
            var sourceEvent = new VacancyReferredEvent
            {
                VacancyReference = ReferredVacancyReferenceNumber,
                VacancyId = _exampleVacancyId
            };
            var @event = JsonConvert.SerializeObject(sourceEvent);
            
            await _sut.HandleAsync(@event);
            
            _mockCommunicationQueueService.Verify(x => x.AddMessageAsync(It.IsAny<CommunicationRequest>()), Times.Never);
        }
    }
}