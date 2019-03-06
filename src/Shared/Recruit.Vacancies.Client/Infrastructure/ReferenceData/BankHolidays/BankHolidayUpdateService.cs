using System;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Services.ReferenceData;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        private readonly IBankHolidayProvider _bankHolidayProvider;

        public BankHolidayUpdateService(IOptions<BankHolidayConfiguration> config, IReferenceDataWriter referenceDataWriter, 
            ILogger<BankHolidayUpdateService> logger, IBankHolidayProvider bankHolidayProvider)
        {
            _config = config.Value;
            _referenceDataWriter = referenceDataWriter;
            _logger = logger;
            _bankHolidayProvider = bankHolidayProvider;
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
            var bankHolidaysFromDb = await _bankHolidayProvider.GetBankHolidayListAsync();
            var bankHolidaysFromApiJson = JsonConvert.SerializeObject(bankHolidaysFromApi.Data);
            var bankHolidaysFromDbJson = JsonConvert.SerializeObject(bankHolidaysFromDb.Data);
            var areEqual = JToken.DeepEquals(bankHolidaysFromApiJson, bankHolidaysFromDbJson);
            if (!areEqual)
                return true;
            return false;
        }
    }

}
