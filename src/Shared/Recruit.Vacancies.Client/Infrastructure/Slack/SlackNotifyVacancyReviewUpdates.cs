using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Services;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Slack
{
    internal class SlackNotifyVacancyReviewUpdates : INotifyVacancyReviewUpdates
    {
        private readonly ISlackClient _slackClient;

        public SlackNotifyVacancyReviewUpdates(ISlackClient slackClient)
        {
            _slackClient = slackClient;
        }

        public Task VacancyReviewCreated(long vacancyReference)
        {
            var message = new SlackMessage { Text = $"Vacancy VAC{vacancyReference} is ready for review" };

            return _slackClient.Post(message, Emojis.New);
        }

        public Task VacancyReviewReferred(long vacancyReference)
        {
            var message = new SlackMessage { Text = $"Vacancy VAC{vacancyReference} has been referred" };

            return _slackClient.Post(message, Emojis.Referred);
        }

        public Task VacancyReviewApproved(long vacancyReference)
        {
             var message = new SlackMessage { Text = $"Vacancy VAC{vacancyReference} has been approved" };

            return _slackClient.Post(message, Emojis.Approved);
        }
    }
}
