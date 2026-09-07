using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi
{
    public class OuterApiClient : IOuterApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly OuterApiConfiguration _config;
        private readonly JsonSerializerOptions _options = new()
        {
            Converters =
            {
                new JsonStringEnumConverter()
            }
        };
        private const string ApiVersion = "1";
        private const string MediaType = "application/json";

        public OuterApiClient (
            HttpClient httpClient, 
            IOptions<OuterApiConfiguration> config)
        {
            _httpClient = httpClient;
            _config = config.Value;
            _httpClient.BaseAddress = new Uri(_config.BaseUrl);
        }

        public async Task<TResponse> Get<TResponse>(IGetApiRequest request)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, request.GetUrl);
            AddHeaders(requestMessage);
            
            var response = await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);

            if (response.StatusCode.Equals(HttpStatusCode.NotFound))
            {
                return default;
            }

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return JsonConvert.DeserializeObject<TResponse>(json);
            }

            response.EnsureSuccessStatusCode();

            return default;
        }

        public async Task Post(IPostApiRequest request, bool ensureSuccessStatusCode = true)
        {
            var stringContent = new StringContent(System.Text.Json.JsonSerializer.Serialize(request.Data, _options), Encoding.UTF8, MediaType);
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, request.PostUrl)
            {
                Content = stringContent,
            };
            AddHeaders(requestMessage);
            var response = await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);
            if (ensureSuccessStatusCode)
            {
                response.EnsureSuccessStatusCode();    
            }
        }

        public async Task<TResponse> Post<TResponse>(IPostApiRequest request)
        {
            var stringContent = new StringContent(System.Text.Json.JsonSerializer.Serialize(request.Data, _options), Encoding.UTF8, MediaType);
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, request.PostUrl)
            {
                Content = stringContent,
            };
            AddHeaders(requestMessage);
            var response = await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);
            
            if (response.StatusCode.Equals(HttpStatusCode.NotFound))
            {
                return default;
            }

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return JsonConvert.DeserializeObject<TResponse>(json);
            }

            response.EnsureSuccessStatusCode();
            return default;
        }

        private void AddHeaders(HttpRequestMessage httpRequestMessage)
        {
            httpRequestMessage.Headers.Add("Ocp-Apim-Subscription-Key", _config.Key);
            httpRequestMessage.Headers.Add("X-Version", ApiVersion);
        }       
    }

    public interface IGetApiRequest
    {
        string GetUrl { get; }
    }

    public interface IPostApiRequest
    {
        [System.Text.Json.Serialization.JsonIgnore]
        string PostUrl { get; }
        object Data { get; set; }
    }
}