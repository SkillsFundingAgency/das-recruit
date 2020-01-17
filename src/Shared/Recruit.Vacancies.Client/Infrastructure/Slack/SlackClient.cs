using System;
using System.Net.Http;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Slack
{
    internal class SlackClient : ISlackClient
    {
        private readonly string _webhookUrl;
        private readonly HttpClient _httpClient;
        private readonly ILogger<SlackClient> _logger;

        private readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver { NamingStrategy = new SnakeCaseNamingStrategy() },
            NullValueHandling = NullValueHandling.Ignore
        };

        public SlackClient(IHttpClientFactory clientFactory, IOptions<SlackConfiguration> slackConfig, ILogger<SlackClient> logger)
        {
            _httpClient = clientFactory.CreateClient();
            _httpClient.Timeout = new TimeSpan(0, 0, 30);

            _webhookUrl = slackConfig.Value.WebHookUrl;
            _logger = logger;
        }

        public async Task PostAsync(SlackMessage message, SlackVacancyNotificationType emoji)
        {
            var emojiIconId = GetEmojiIconId(emoji);
            message.Text = $"{emojiIconId} {message.Text}";

            var payload = SerializePayload(message);

            using (var response = await _httpClient.PostAsync(_webhookUrl, new StringContent(payload)))
            {
                var content = await response.Content.ReadAsStringAsync();

                var success = content.Equals("ok", StringComparison.OrdinalIgnoreCase);

                if (!success)
                    _logger.LogWarning($"Failed to send notification to Slack with url: {_webhookUrl}");
            }
        }

        private string SerializePayload(SlackMessage message)
        {
            return JsonConvert.SerializeObject(message, _jsonSerializerSettings);
        }

        private string GetEmojiIconId(SlackVacancyNotificationType emoji)
        {
            switch (emoji)
            {
                case SlackVacancyNotificationType.New:
                    return ":sparkle:";
                case SlackVacancyNotificationType.Approved:
                    return ":heavy_check_mark:";
                case SlackVacancyNotificationType.Referred:
                    return ":x:";
                case SlackVacancyNotificationType.ManuallyClosed:
                    return ":negative_squared_cross_mark:";
                case SlackVacancyNotificationType.Extended:
                    return ":date:";
                default:
                    return ":question:";
            }
        }
    }
}
