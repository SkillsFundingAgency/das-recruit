using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Network.Default;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Services.ReferenceData;
using JsonDiffPatchDotNet;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.BankHolidays
{
    public class BankHolidayUpdateService : IBankHolidayUpdateService
    {
        private readonly BankHolidayConfiguration _config;
        private readonly IReferenceDataWriter _referenceDataWriter;
        private readonly ILogger<BankHolidayUpdateService> _logger;
        private readonly IBankHolidayProvider _bankholidayProvider;

        public BankHolidayUpdateService(IOptions<BankHolidayConfiguration> config, IReferenceDataWriter referenceDataWriter, 
            ILogger<BankHolidayUpdateService> logger, IBankHolidayProvider bankholidayProvider)
        {
            _config = config.Value;
            _referenceDataWriter = referenceDataWriter;
            _logger = logger;
            _bankholidayProvider = bankholidayProvider;
        }

        public async Task UpdateBankHolidaysAsync()
        {
            var client = new RestClient(_config.Url);
            var request = new RestRequest();
            var response = await client.ExecuteTaskAsync<BankHolidays.BankHolidaysData>(request);
           
            if (!response.IsSuccessful)
                throw new Exception($"Error getting list of bank holidays from url:{_config.Url}. Error:{response.ErrorMessage}");

            if (!response.Data.EnglandAndWales.Events.Any())
                throw new Exception($"Expected a non-empty list of bank holidays from url:{_config.Url}");

            var bankHolidaysFromApi = new BankHolidays
            {
                Data = response.Data,             
            };

            if (await ValidateBankHolidayData(bankHolidaysFromApi))
            {
                await _referenceDataWriter.UpsertReferenceData(bankHolidaysFromApi);
                _logger.LogInformation($"Upserted bank holidays into ReferenceData store. Last England and Wales date:{bankHolidaysFromApi.Data.EnglandAndWales.Events.Last().Date}");
            }
            _logger.LogInformation("ReferenceData not updated as the ETag value is the same.");
        }

        private async Task<bool> ValidateBankHolidayData(BankHolidays bankHolidaysFromApi)
        {
            var bankHolidaysFromDb = await _bankholidayProvider.GetBankHolidayListAsync();
            var bankHolidaysFromApiJson = JsonConvert.SerializeObject(bankHolidaysFromApi.Data);
            var bankHolidaysFromDbJson = JsonConvert.SerializeObject(bankHolidaysFromDb.Data);
            var equals = JToken.DeepEquals(bankHolidaysFromApiJson, bankHolidaysFromDbJson);
            if (!equals)
                return true;
            return false;
        }
    }

}
