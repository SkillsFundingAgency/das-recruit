using System;
using System.Linq;
using System.Threading.Tasks;
using Communication.Types;
using Esfa.Recruit.Vacancies.Client.Application.Communications;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;
using Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Vacancy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Esfa.Recruit.UnitTests.Jobs.DomainEventHandlers
{
    public class VacancyReferredDomainEventHandlerTests
    {
        private const long ReferredVacancyReferenceNumber = 11111111;
        private readonly Guid _exampleVacancyId = Guid.NewGuid();
        private readonly VacancyReferredDomainEventHandler _sut;
        private readonly Mock<ICommunicationQueueService> _mockCommunicationQueueService;
        private CommunicationRequest _sentCommRequest;

        public VacancyReferredDomainEventHandlerTests()
        {
            _mockCommunicationQueueService = new Mock<ICommunicationQueueService>();
            _mockCommunicationQueueService.Setup(x => x.AddMessageAsync(It.IsAny<CommunicationRequest>()))
                            .Returns(Task.CompletedTask)
                            .Callback<CommunicationRequest>(cr => _sentCommRequest = cr);

            _sut = new VacancyReferredDomainEventHandler(Mock.Of<ILogger<VacancyReferredDomainEventHandler>>(),
                                                        _mockCommunicationQueueService.Object);
        }

        [Fact]
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

        [Fact]
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
    }
}