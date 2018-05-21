using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Slack;
using Moq;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Infrastructure.Slack
{
    public class SlackNotifyVacancyReviewUpdatesTests
    {
        [Fact]
        public async Task WhenPostingVacancyReadyForReviewMessage_ThenItShouldPostAMessageWithTheNewIconAndSuitableMessage()
        {
            var vacancyReference = 1000000001;
            var mockSlackClient = new Mock<ISlackClient>();

            mockSlackClient
                .Setup(c => c.Post(
                    It.Is<SlackMessage>(s => s.Text.Contains("ready for review") && s.Text.Contains(vacancyReference.ToString())), 
                    It.Is<Emojis>(e => e.Equals(Emojis.New))))
                    .Returns(Task.CompletedTask)
                    .Verifiable();

            var client = new SlackNotifyVacancyReviewUpdates(mockSlackClient.Object);

            await client.VacancyReviewCreated(vacancyReference);

            mockSlackClient.Verify();
        }

        [Fact]
        public async Task WhenPostingVacancyApprovedMessage_ThenItShouldPostAMessageWithTheNewIconAndSuitableMessage()
        {
            var vacancyReference = 1000000001;
            var mockSlackClient = new Mock<ISlackClient>();

            mockSlackClient
                .Setup(c => c.Post(
                    It.Is<SlackMessage>(s => s.Text.Contains("has been approved") && s.Text.Contains(vacancyReference.ToString())), 
                    It.Is<Emojis>(e => e.Equals(Emojis.Approved))))
                    .Returns(Task.CompletedTask)
                    .Verifiable();

            var client = new SlackNotifyVacancyReviewUpdates(mockSlackClient.Object);

            await client.VacancyReviewApproved(vacancyReference);

            mockSlackClient.Verify();
        }

        [Fact]
        public async Task WhenPostingVacancyReferredMessage_ThenItShouldPostAMessageWithTheNewIconAndSuitableMessage()
        {
            var vacancyReference = 1000000001;
            var mockSlackClient = new Mock<ISlackClient>();

            mockSlackClient
                .Setup(c => c.Post(
                    It.Is<SlackMessage>(s => s.Text.Contains("has been referred") && s.Text.Contains(vacancyReference.ToString())), 
                    It.Is<Emojis>(e => e.Equals(Emojis.Referred))))
                    .Returns(Task.CompletedTask)
                    .Verifiable();

            var client = new SlackNotifyVacancyReviewUpdates(mockSlackClient.Object);

            await client.VacancyReviewReferred(vacancyReference);

            mockSlackClient.Verify();
        }
    }
}