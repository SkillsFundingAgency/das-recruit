using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SFA.DAS.Http;
using SFA.DAS.Http.TokenGenerators;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.TrainingProviderAgreement
{
    public class TrainingProviderAgreementProvider : ITrainingProviderAgreementProvider
    {
        private const string AgreementStatusAgreed = "Agreed";

        private readonly PasAccountApiConfiguration _config;

        public TrainingProviderAgreementProvider(IOptions<PasAccountApiConfiguration> config)
        {
            _config = config.Value;
        }

        public async Task<bool> HasAgreementAsync(long ukprn)
        {
            using (var httpClient = CreateHttpClient())
            {
                var requestUri = Path.Combine(_config.ApiBaseUrl, $"api/account/{ukprn}/agreement");
                var response = await httpClient.GetStringAsync(requestUri);

                var agreement = JsonConvert.DeserializeObject<Agreement>(response);

                return agreement.Status == AgreementStatusAgreed;
            }
        }

        private HttpClient CreateHttpClient()
        {
            var httpClient = new HttpClientBuilder()
                .WithDefaultHeaders()
                .WithBearerAuthorisationHeader(new AzureADBearerTokenGenerator(_config))
                .Build();

            httpClient.BaseAddress = new Uri(_config.ApiBaseUrl);

            return httpClient;
        }
    }
}
