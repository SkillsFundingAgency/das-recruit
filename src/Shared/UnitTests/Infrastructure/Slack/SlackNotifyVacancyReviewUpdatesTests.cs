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

            await client.NewVacancyReview(vacancyReference);

            mockSlackClient.Verify();
        }
    }
}