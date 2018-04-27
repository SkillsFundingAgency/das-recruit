using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Slack
{
    internal interface ISlackClient
    {
        Task Post(SlackMessage message, Emojis emoji);
    }
}
