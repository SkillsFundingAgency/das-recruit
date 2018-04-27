using System;
using System.Net.Http;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Exceptions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Slack
{
    internal class SlackClient : ISlackClient
    {
        private readonly string _webhookUrl;
        private static readonly HttpClient _httpClient = new HttpClient { Timeout = new TimeSpan(0, 0, 30) };

        public SlackClient(IOptions<SlackConfiguration> slackConfig)
        {
            _webhookUrl = slackConfig.Value.WebHookUrl;
        }

        public async Task Post(SlackMessage message, Emojis emoji)
        {
            var emojiName = GetEmojiName(emoji);
            message.Text = emojiName + " " + message.Text;

            var payload = SerializePayload(message);

            using (var response = await _httpClient.PostAsync(_webhookUrl, new StringContent(payload)))
            {
                var content = await response.Content.ReadAsStringAsync();

                var success = content.Equals("ok", StringComparison.OrdinalIgnoreCase);

                if (!success)
                    throw new NotificationException($"Failed to send notification to Slack with url: {_webhookUrl}");
            }
        }

        private static string SerializePayload(SlackMessage message)
        {
            var resolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            };

            return JsonConvert.SerializeObject(message, new JsonSerializerSettings
            {
                ContractResolver = resolver,
                NullValueHandling = NullValueHandling.Ignore
            });
        }

        private static string GetEmojiName(Emojis emoji)
        {
            switch (emoji)
            {
                case Emojis.New:
                    return ":sparkle:";
                case Emojis.Approved:
                    return ":heavy_check_mark:";
                case Emojis.Referred:
                    return ":x:";
                default:
                    return ":question:";
            }
        }
    }
}
