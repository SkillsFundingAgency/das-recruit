using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Slack;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Esfa.Recruit.UnitTests.Vacancies.Client.Infrastructure.Slack
{
    public class SlackNotifyVacancyUpdatesTests
    {
        [Fact]
        public async Task WhenVacancyClosedByUser_ThenASlackMessageShouldBePostedWithTheNewIconAndSuitableMessage()
        {
            var vacancyReference = 1000000001;
            var vacancy = new Vacancy{VacancyReference = vacancyReference, OwnerType = OwnerType.Employer, ClosedByUser = new VacancyUser()};
            var mockSlackClient = new Mock<ISlackClient>();

            mockSlackClient
                .Setup(c => c.PostAsync(
                    It.Is<SlackMessage>(s => 
                        s.Text.Contains("has been closed") && 
                        s.Text.Contains(" (Employer)") && 
                        s.Text.Contains(vacancyReference.ToString())),
                    It.Is<SlackVacancyNotificationType>(e => e.Equals(SlackVacancyNotificationType.ManuallyClosed))))
                    .Returns(Task.CompletedTask)
                    .Verifiable();

            var client = new SlackNotifyVacancyUpdates(mockSlackClient.Object, Options.Create(new SlackConfiguration()));

            await client.VacancyManuallyClosed(vacancy);

            mockSlackClient.Verify();
        }

        [Fact]
        public async Task WhenVacancyDatExtendedByUser_ThenASlackMessageShouldBePostedWithTheNewIconAndSuitableMessage()
        {
            var vacancyReference = 1000000001;
            var vacancy = new Vacancy{VacancyReference = vacancyReference, OwnerType = OwnerType.Employer};
            var mockSlackClient = new Mock<ISlackClient>();

            mockSlackClient
                .Setup(c => c.PostAsync(
                    It.Is<SlackMessage>(s => 
                        s.Text.Contains("has been extended") && 
                        s.Text.Contains(" (Employer)") && 
                        s.Text.Contains(vacancyReference.ToString())),
                    It.Is<SlackVacancyNotificationType>(e => e.Equals(SlackVacancyNotificationType.Extended))))
                    .Returns(Task.CompletedTask)
                    .Verifiable();

            var client = new SlackNotifyVacancyUpdates(mockSlackClient.Object, Options.Create(new SlackConfiguration()));

            await client.LiveVacancyChanged(vacancy);

            mockSlackClient.Verify();
        }
    }
}