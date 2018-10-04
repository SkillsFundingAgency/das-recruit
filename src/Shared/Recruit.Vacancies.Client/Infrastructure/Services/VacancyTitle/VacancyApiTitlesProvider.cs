using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Recruit.Vacancies.Client.Infrastructure.Configuration;

namespace Recruit.Vacancies.Client.Infrastructure.Services.VacancyTitle
{
    public class VacancyApiTitlesProvider : IGetVacancyTitlesProvider
    {
        private readonly string _vacancyApiReadSubscriptionKey;
        private readonly HttpClient _httpClient;
        private readonly ILogger<VacancyApiTitlesProvider> _logger;
        private readonly string _baseSearchEndpoint;
        private const string SubscriptionKeyHeaderKey = "Ocp-Apim-Subscription-Key";
        private const int MaxSearchResultsPerPageLimit = 250;
        private const int NoOfPagesToSearch = 4;

        public VacancyApiTitlesProvider(ILogger<VacancyApiTitlesProvider> logger, IOptions<VacancyApiConfiguration> vacancyApiConfig)
        {
            _logger = logger;
            _baseSearchEndpoint = vacancyApiConfig.Value.ApiSearchBaseUrl;
            _vacancyApiReadSubscriptionKey = vacancyApiConfig.Value.ApiReadSubscriptionKey;
            _httpClient = new HttpClient();
        }

        public async Task<IList<string>> GetVacancyTitlesAsync(string larsCode)
        {
            var frameworkCodeIdentifier = "-";
            var isFrameworkCode = larsCode.Contains(frameworkCodeIdentifier);

            if (isFrameworkCode)
                larsCode = larsCode.Substring(0, larsCode.IndexOf(frameworkCodeIdentifier));

            var vacancyApiSearchUrlFormat = $"{_baseSearchEndpoint}?{(isFrameworkCode ? "frameworkLarsCodes" : "standardLarsCodes")}={{0}}&pageSize={{1}}&pageNumber={{2}}";

            var titles = new List<string>();


            _httpClient.DefaultRequestHeaders.Add(SubscriptionKeyHeaderKey, _vacancyApiReadSubscriptionKey);

            for (var reqPageNo = 1; reqPageNo <= NoOfPagesToSearch; reqPageNo++)
            {
                try
                {
                    var url = string.Format(vacancyApiSearchUrlFormat, larsCode, MaxSearchResultsPerPageLimit, reqPageNo);
                    var respContent = await _httpClient.GetStringAsync(url);

                    var resp = JsonConvert.DeserializeObject<VacancyApiSearchResponse>(respContent);

                    titles.AddRange(resp.Results.Select(r => r.Title));

                    if (resp.TotalReturned < MaxSearchResultsPerPageLimit)
                    {
                        break;
                    }
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "Error trying to retrieve titles.", null);
                }
                catch (JsonReaderException ex)
                {
                    _logger.LogError(ex, $"Couldn't deserialise {nameof(VacancyApiSearchResponse)}.", null);
                }
            }

            return titles;
        }
    }
}