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

namespace Recruit.Vacancies.Jobs.UnitTests.DomainEvents.Handlers
{
    public class VacancyReviewedHandlerTests
    {
        private const long _vacancyReference = 11111111;
        private const long _exampleUkprn = 12345678;
        private readonly Guid _exampleVacancyId = Guid.NewGuid();
        private readonly string _employerAccountId = "VJWCD";
        private readonly VacancyReviewedHandler _sut;
        private readonly Mock<ICommunicationQueueService> _mockCommunicationQueueService;
        private CommunicationRequest _sentCommRequest;

        public VacancyReviewedHandlerTests()
        {
            _mockCommunicationQueueService = new Mock<ICommunicationQueueService>();
            _mockCommunicationQueueService.Setup(x => x.AddMessageAsync(It.IsAny<CommunicationRequest>()))
                            .Returns(Task.CompletedTask)
                            .Callback<CommunicationRequest>(cr => _sentCommRequest = cr);

            _sut = new VacancyReviewedHandler(Mock.Of<ILogger<VacancyReviewedHandler>>(),
                                                        _mockCommunicationQueueService.Object);
        }

        [Fact]
        public async Task GivenVacancyReviewedEvent_VerifyCommunicationRequestIsSentToCommunicationQueue()
        {
            //Arrange
            var sourceEvent = new VacancyReviewedEvent
            {
                EmployerAccountId = _employerAccountId,
                VacancyReference = _vacancyReference,
                VacancyId = _exampleVacancyId,
                ukprn = _exampleUkprn
            };

            var @event = JsonConvert.SerializeObject(sourceEvent);

            //Act
            await _sut.HandleAsync(@event);

            //Assert
            _mockCommunicationQueueService.Verify(x => x.AddMessageAsync(It.IsAny<CommunicationRequest>()), Times.Once);
        }

        [Fact]
        public async Task GivenVacancyReviewedEvent_VerifyCommunicationRequestHasExpectedData()
        {
            //Arrange
            var sourceEvent = new VacancyReviewedEvent
            {
                EmployerAccountId = _employerAccountId,
                VacancyReference = _vacancyReference,
                VacancyId = _exampleVacancyId,
                ukprn = _exampleUkprn
            };

            var @event = JsonConvert.SerializeObject(sourceEvent);

            //Act
            await _sut.HandleAsync(@event);

            //Assert
            _sentCommRequest
                .Entities
                .Any(cr => cr.EntityType.Equals(CommunicationConstants.EntityTypes.Vacancy) && ((long)cr.EntityId == _vacancyReference))
                .Should()
                .BeTrue();
        }
    }
}