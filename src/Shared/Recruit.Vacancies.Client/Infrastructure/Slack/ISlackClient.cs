using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Slack
{
    internal interface ISlackClient
    {
        Task PostAsync(SlackMessage message, SlackVacancyNotificationType emoji);
    }
}
