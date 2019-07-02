﻿using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SFA.DAS.Http;
using SFA.DAS.Http.TokenGenerators;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.PasAccount
{
    public class PasAccountClient : IPasAccountClient
    {
        private const string AgreementStatusAgreed = "Agreed";

        private readonly PasAccountApiConfiguration _config;

        public PasAccountClient(IOptions<PasAccountApiConfiguration> config)
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
