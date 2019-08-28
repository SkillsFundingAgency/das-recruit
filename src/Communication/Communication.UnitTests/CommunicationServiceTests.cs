using System.Linq;
using System.Threading.Tasks;
using Communication.Core;
using Communication.Types;
using Esfa.Recruit.Vacancies.Client.Application.Communications;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Communication.UnitTests
{
    public class CommunicationServiceTests
    {
        private readonly Mock<ICommunicationProcessor> _mockProcessor;
        private readonly Mock<ICommunicationRepository> _mockRepository;
        private readonly Mock<IAggregateCommunicationComposeQueuePublisher> _mockComposeQueuePublisher;
        private readonly Mock<IDispatchQueuePublisher> _mockDispatchQueuePublisher;
        private readonly CommunicationService _sut;

        private CommunicationRequest _arbitraryCommRequest = new CommunicationRequest(CommunicationConstants.RequestType.VacancyRejected, string.Empty, string.Empty);

        public CommunicationServiceTests()
        {
            _mockProcessor = new Mock<ICommunicationProcessor>();
            _mockRepository = new Mock<ICommunicationRepository>();
            _mockComposeQueuePublisher = new Mock<IAggregateCommunicationComposeQueuePublisher>();
            _mockDispatchQueuePublisher = new Mock<IDispatchQueuePublisher>();
            _sut = new CommunicationService(Mock.Of<ILogger<CommunicationService>>(), _mockProcessor.Object,
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
    }
}