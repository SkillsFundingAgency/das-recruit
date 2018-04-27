using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Services;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Slack
{
    internal class VacancyReviewSlackClient : INotifyVacancyReviewUpdates
    {
        private readonly ISlackClient _slackClient;

        public VacancyReviewSlackClient(ISlackClient slackClient)
        {
            _slackClient = slackClient;
        }

        public Task NewVacancyReview(long vacancyReference)
        {
            var message = new SlackMessage {Text = $"Vacancy VAC{vacancyReference} is ready for review"};

            return _slackClient.Post(message, Emojis.New);
        }
    }
}
