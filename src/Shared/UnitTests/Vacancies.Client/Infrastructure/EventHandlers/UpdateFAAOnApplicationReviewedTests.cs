using System;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.EventHandlers;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.FAA;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Esfa.Recruit.UnitTests.Vacancies.Client.Infrastructure.EventHandlers
{
    public class UpdateFaaOnApplicationReviewedTests
    {
        [Theory]
        [InlineData(ApplicationReviewStatus.New, false)]
        [InlineData(ApplicationReviewStatus.Successful, true)]
        [InlineData(ApplicationReviewStatus.Unsuccessful, true)]    
        public async Task Handle_ShouldSubmitMessageToFAAMessageQueueDependingOnStatus(ApplicationReviewStatus status, bool shouldPublishFaaServiceBusMessage)
        {
            FaaApplicationStatusSummary actualFaaApplicationStatusSummary = null;

            var faaServiceMock = new Mock<IFaaService>();
            faaServiceMock.Setup(f => f.PublishApplicationStatusSummaryAsync(It.IsAny<FaaApplicationStatusSummary>()))
                .Callback<FaaApplicationStatusSummary>(f => actualFaaApplicationStatusSummary = f)
                .Returns(Task.CompletedTask);

            var sut = new UpdateFaaOnApplicationReviewed(faaServiceMock.Object, new Mock<ILogger<UpdateFaaOnApplicationReviewed>>().Object);

            var @event = new ApplicationReviewedEvent
            {
                CandidateFeedback = "candidate feedback",
                CandidateId = Guid.NewGuid(),
                Status = status,
                VacancyReference = 1234567890
            };

            await sut.Handle(@event, new CancellationToken());

            faaServiceMock.Verify(f => f.PublishApplicationStatusSummaryAsync(It.IsAny<FaaApplicationStatusSummary>()), 
                Times.Exactly(shouldPublishFaaServiceBusMessage ? 1 : 0) );

            if (actualFaaApplicationStatusSummary != null)
            {
                actualFaaApplicationStatusSummary.CandidateId.Should().Be(@event.CandidateId);
                actualFaaApplicationStatusSummary.ApplicationStatus.Should().Be(status == ApplicationReviewStatus.Successful ? FaaApplicationStatus.Successful : FaaApplicationStatus.Unsuccessful);
                actualFaaApplicationStatusSummary.IsRecruitVacancy.Should().Be(true);
                actualFaaApplicationStatusSummary.UnsuccessfulReason.Should().Be(@event.CandidateFeedback);
                actualFaaApplicationStatusSummary.VacancyReference.Should().Be((int)@event.VacancyReference);
            }
        }
    }
}
