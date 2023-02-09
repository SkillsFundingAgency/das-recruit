using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SFA.DAS.Http;
using SFA.DAS.Http.TokenGenerators;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.PasAccount
{
    public class PasAccountProvider : IPasAccountProvider
    {
        private const string AgreementStatusAgreed = "Agreed";

        private readonly PasAccountApiConfiguration _config;

        public PasAccountProvider(IOptions<PasAccountApiConfiguration> config)
        {
            _config = config.Value;
        }

        public async Task<bool> HasAgreementAsync(long ukprn)
        {
            if (ukprn == EsfaTestTrainingProvider.Ukprn)
                return true;

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
                .WithManagedIdentityAuthorisationHeader(new ManagedIdentityTokenGenerator(_config))
                .Build();

            httpClient.BaseAddress = new Uri(_config.ApiBaseUrl);

            return httpClient;
        }
    }
}
