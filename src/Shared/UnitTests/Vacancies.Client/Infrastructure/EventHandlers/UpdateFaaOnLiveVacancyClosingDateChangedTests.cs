using System;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.EventHandlers;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.FAA;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Esfa.Recruit.UnitTests.Vacancies.Client.Infrastructure.EventHandlers
{
    public class UpdateFaaOnLiveVacancyClosingDateChangedTests
    {
        private Mock<ILogger<UpdateFaaOnLiveVacancyClosingDateChanged>> _mockLogger;
        private Mock<IFaaService> _mockFaaService;
        private UpdateFaaOnLiveVacancyClosingDateChanged _handler;

        [Fact]
        public async Task ShouldSendMessageToFaa()
        {
            var @event = new LiveVacancyClosingDateChangedEvent
            {
                VacancyId = Guid.NewGuid(),
                VacancyReference = 299792458,
                NewClosingDate = DateTime.UtcNow
            };

            await _handler.Handle(@event, CancellationToken.None);

            _mockFaaService
                .Verify(
                    x => x.PublishVacancyStatusSummaryAsync(
                        It.Is<FaaVacancyStatusSummary>(p =>
                            p.ClosingDate == @event.NewClosingDate
                            && p.LegacyVacancyId == @event.VacancyReference
                            && p.VacancyStatus == FaaVacancyStatuses.Live
                        )),
                    Times.Once);
        }

        public UpdateFaaOnLiveVacancyClosingDateChangedTests()
        {
            _mockLogger = new Mock<ILogger<UpdateFaaOnLiveVacancyClosingDateChanged>>();
            _mockFaaService = new Mock<IFaaService>();
            _handler = new UpdateFaaOnLiveVacancyClosingDateChanged(_mockFaaService.Object, _mockLogger.Object);
        }
    }
}
