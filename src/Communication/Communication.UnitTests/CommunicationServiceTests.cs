using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Communication.Core;
using Communication.Types;
using Esfa.Recruit.Vacancies.Client.Application.Communications;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Communication.UnitTests
{
    public class CommunicationServiceTests
    {
        private readonly Mock<ICommunicationProcessor> _mockProcessor;
        private readonly Mock<IAggregateCommunicationProcessor> _mockAggregateProcessor;
        private readonly Mock<ICommunicationRepository> _mockRepository;
        private readonly Mock<IAggregateCommunicationComposeQueuePublisher> _mockComposeQueuePublisher;
        private readonly Mock<IDispatchQueuePublisher> _mockDispatchQueuePublisher;
        private readonly CommunicationService _sut;

        private CommunicationRequest _arbitraryCommRequest = new CommunicationRequest(CommunicationConstants.RequestType.VacancyRejected, string.Empty, string.Empty);

        public CommunicationServiceTests()
        {
            _mockProcessor = new Mock<ICommunicationProcessor>();
            _mockAggregateProcessor = new Mock<IAggregateCommunicationProcessor>();
            _mockRepository = new Mock<ICommunicationRepository>();
            _mockComposeQueuePublisher = new Mock<IAggregateCommunicationComposeQueuePublisher>();
            _mockDispatchQueuePublisher = new Mock<IDispatchQueuePublisher>();
            _sut = new CommunicationService(Mock.Of<ILogger<CommunicationService>>(), _mockProcessor.Object, _mockAggregateProcessor.Object,
                                            _mockRepository.Object, _mockComposeQueuePublisher.Object, _mockDispatchQueuePublisher.Object);
        }

        [Fact]
        public async Task GivenImmediateFrequencyCommunicationMessages_ShouldQueueMessagesForDispatchImmediately()
        {
            const int SampleCommunicationMessagesCount = 3;
            var sampleCommunicationMessages = Enumerable.Repeat(new CommunicationMessage { Frequency = DeliveryFrequency.Immediate }, SampleCommunicationMessagesCount);

            _mockProcessor.Setup(p => p.CreateMessagesAsync(_arbitraryCommRequest))
                            .ReturnsAsync(sampleCommunicationMessages);

            await _sut.ProcessCommunicationRequestAsync(_arbitraryCommRequest);

            _mockRepository.Verify(r => r.InsertAsync(It.IsIn(sampleCommunicationMessages)), Times.Exactly(SampleCommunicationMessagesCount));
            _mockDispatchQueuePublisher.Verify(d => d.AddMessageAsync(It.IsAny<CommunicationMessageIdentifier>()), Times.Exactly(SampleCommunicationMessagesCount));
        }

        [Theory]
        [InlineData(DeliveryFrequency.Default)]
        [InlineData(DeliveryFrequency.Daily)]
        [InlineData(DeliveryFrequency.Weekly)]
        public async Task GivenNonImmediateFrequencyCommunicationMessages_ShouldNotQueueMessagesForDispatchImmediately(DeliveryFrequency argFrequency)
        {
            const int SampleCommunicationMessagesCount = 3;
            var sampleCommunicationMessages = Enumerable.Repeat(new CommunicationMessage { Frequency = argFrequency }, SampleCommunicationMessagesCount);

            _mockProcessor.Setup(p => p.CreateMessagesAsync(_arbitraryCommRequest))
                            .ReturnsAsync(sampleCommunicationMessages);

            await _sut.ProcessCommunicationRequestAsync(_arbitraryCommRequest);

            _mockRepository.Verify(r => r.InsertAsync(It.IsIn(sampleCommunicationMessages)), Times.Exactly(SampleCommunicationMessagesCount));
            _mockDispatchQueuePublisher.Verify(d => d.AddMessageAsync(It.IsAny<CommunicationMessageIdentifier>()), Times.Never);
        }

        [Fact]
        public async Task GivenAggregateCommunicationRequestForImmediateDelivery_ShouldThrowError()
        {
            var aggCommRequest = new AggregateCommunicationRequest(Guid.NewGuid(), CommunicationConstants.RequestType.ApplicationSubmitted,
                                                                    DeliveryFrequency.Immediate, DateTime.UtcNow, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow);
            var ex = await Assert.ThrowsAsync<NotSupportedException>(() => _sut.ProcessAggregateCommunicationRequestAsync(aggCommRequest));
            ex.Message.Should().BeEquivalentTo(CommunicationErrorMessages.InvalidAggregateCommunicationRequestedFrequencyMessage);
        }

        [Fact]
        public async Task GivenAggregateCommunicationRequestFor5ApplicationsSubmittedFor3Recipients_ShouldQueue3ComposeMessages()
        {
            var recipientOne = new CommunicationUser("1", "a@a.com", "a", "person", UserParticipation.PrimaryUser, Guid.NewGuid().ToString());
            var recipientTwo = new CommunicationUser("2", "b@a.com", "b", "person", UserParticipation.PrimaryUser, Guid.NewGuid().ToString());
            var recipientThree = new CommunicationUser("3", "c@a.com", "c", "person", UserParticipation.PrimaryUser, Guid.NewGuid().ToString());
            var messages = Enumerable.Concat(Enumerable.Concat(Enumerable.Repeat(new CommunicationMessage() { Recipient = recipientOne }, 3),
                        Enumerable.Repeat(new CommunicationMessage() { Recipient = recipientTwo }, 3)),
                        Enumerable.Repeat(new CommunicationMessage() { Recipient = recipientThree }, 3));
            var from = DateTime.UtcNow.AddDays(-1);
            var to = DateTime.UtcNow;
            _mockRepository.Setup(x => x.GetScheduledMessagesSinceAsync(CommunicationConstants.RequestType.ApplicationSubmitted, DeliveryFrequency.Daily, from, to))
                            .ReturnsAsync(messages);
            var aggCommRequest = new AggregateCommunicationRequest(Guid.NewGuid(), CommunicationConstants.RequestType.ApplicationSubmitted, DeliveryFrequency.Daily, DateTime.UtcNow, from, to);

            await _sut.ProcessAggregateCommunicationRequestAsync(aggCommRequest);

            _mockRepository.Verify(x => x.GetScheduledMessagesSinceAsync(aggCommRequest.RequestType, aggCommRequest.Frequency, aggCommRequest.FromDateTime, aggCommRequest.ToDateTime), Times.Once);
            _mockComposeQueuePublisher.Verify(x => x.AddMessageAsync(It.IsAny<AggregateCommunicationComposeRequest>()), Times.Exactly(3));
        }

        [Fact]
        public async Task GivenAggregateCommunicationComposeRequest_AndNoMatchingCommunicationMessages_ShouldNotQueueDispatchOfMessage()
        {
            var msgIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
            var from = DateTime.UtcNow.AddDays(-1);
            var to = DateTime.UtcNow;
            var req = new AggregateCommunicationComposeRequest("UserId", msgIds,
                            new AggregateCommunicationRequest(Guid.NewGuid(), CommunicationConstants.RequestType.ApplicationSubmitted, DeliveryFrequency.Daily, DateTime.UtcNow, from, to));

            _mockRepository.Setup(x => x.GetManyAsync(msgIds))
                            .ReturnsAsync(Enumerable.Empty<CommunicationMessage>());
            await _sut.ProcessAggregateCommunicationComposeRequestAsync(req);

            _mockDispatchQueuePublisher.Verify(x => x.AddMessageAsync(It.IsAny<CommunicationMessageIdentifier>()), Times.Never);
        }

        [Fact]
        public async Task GivenAggregateCommunicationComposeRequest_ShouldQueueDispatchOfOneMessage()
        {
            var msgIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
            var from = DateTime.UtcNow.AddDays(-1);
            var to = DateTime.UtcNow;
            var req = new AggregateCommunicationComposeRequest("UserId", msgIds, new AggregateCommunicationRequest(Guid.NewGuid(), CommunicationConstants.RequestType.ApplicationSubmitted, DeliveryFrequency.Daily, DateTime.UtcNow, from, to));
            var messages = Enumerable.Repeat(new CommunicationMessage(), 10);
            _mockRepository.Setup(x => x.GetManyAsync(msgIds)).ReturnsAsync(messages);
            var aggregateMessage = new CommunicationMessage { Id = Guid.NewGuid() } ;
            _mockAggregateProcessor.Setup(x => x.CreateAggregateMessageAsync(req.AggregateCommunicationRequest, messages))
                                    .ReturnsAsync(aggregateMessage);
            await _sut.ProcessAggregateCommunicationComposeRequestAsync(req);

            _mockRepository.Verify(x => x.InsertAsync(aggregateMessage), Times.Once);
            _mockRepository.Verify(x => x.UpdateScheduledMessagesAsSentAsync(msgIds, aggregateMessage.Id), Times.AtMostOnce);
            _mockDispatchQueuePublisher.Verify(x => x.AddMessageAsync(new CommunicationMessageIdentifier(aggregateMessage.Id)), Times.AtMostOnce);
        }
    }
}