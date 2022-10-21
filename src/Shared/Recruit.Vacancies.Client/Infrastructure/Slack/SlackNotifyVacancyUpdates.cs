using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.Extensions.Options;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Slack
{
    internal class SlackNotifyVacancyUpdates : INotifyVacancyUpdates
    {
        private readonly string _findAnApprenticeshipDetailPrefixUrl;
        private readonly ISlackClient _slackClient;

        public SlackNotifyVacancyUpdates(ISlackClient slackClient, IOptions<SlackConfiguration> slackConfig)
        {
            _slackClient = slackClient;
            _findAnApprenticeshipDetailPrefixUrl = slackConfig.Value.FindAnApprenticeshipDetailPrefixUrl;
        }

        public Task VacancyManuallyClosed(Vacancy vacancy)
        {
            var messageBody = string.Format($"Vacancy VAC{vacancy.VacancyReference} has been closed ({vacancy.OwnerType.ToString()})");

            var message = new SlackMessage {Text = messageBody};

            return _slackClient.PostAsync(message, SlackVacancyNotificationType.ManuallyClosed);
        }

        public Task LiveVacancyChanged(Vacancy vacancy)
        {
            var messageBody = string.Format("Vacancy <{0}{1}|VAC{1}> has been extended ({2})", _findAnApprenticeshipDetailPrefixUrl, vacancy.VacancyReference, vacancy.OwnerType.ToString());

            var message = new SlackMessage {Text = messageBody};

            return _slackClient.PostAsync(message, SlackVacancyNotificationType.Extended);
        }
    }
}
