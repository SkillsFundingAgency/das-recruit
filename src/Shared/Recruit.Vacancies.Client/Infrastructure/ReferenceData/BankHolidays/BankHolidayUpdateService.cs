using System;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly IReferenceDataWriter _referenceDataWriter;
        private readonly ILogger<BankHolidayUpdateService> _logger;        
        private readonly IReferenceDataReader _referenceDataReader;

        public BankHolidayUpdateService( IReferenceDataWriter referenceDataWriter, 
            ILogger<BankHolidayUpdateService> logger, IReferenceDataReader referenceDataReader)
        {
            _referenceDataWriter = referenceDataWriter;
            _logger = logger;            
            _referenceDataReader = referenceDataReader;
        }

        public async Task UpdateBankHolidaysAsync()
        {
            var client = new RestClient(BankHolidayConfiguration.Url);
            var request = new RestRequest();
            var response = await client.ExecuteTaskAsync<BankHolidays.BankHolidaysData>(request);
           
            if (!response.IsSuccessful)
                throw new Exception($"Error getting list of bank holidays from url:{BankHolidayConfiguration.Url}. Error:{response.ErrorMessage}");

            if (!response.Data.EnglandAndWales.Events.Any())
                throw new Exception($"Expected a non-empty list of bank holidays from url:{BankHolidayConfiguration.Url}");

            var bankHolidaysFromApi = new BankHolidays
            {
                Data = response.Data             
            };

            if (await HasBankHolidayDataChanged(bankHolidaysFromApi))
            {
                await _referenceDataWriter.UpsertReferenceData(bankHolidaysFromApi);
                _logger.LogInformation($"Upserted bank holidays into ReferenceData store. Last England and Wales date:{bankHolidaysFromApi.Data.EnglandAndWales.Events.Last().Date}");
            }
            else
                _logger.LogInformation("ReferenceData not updated as there is no change.");
        }

        private async Task<bool> HasBankHolidayDataChanged(BankHolidays bankHolidaysFromApi)
        {
            var bankHolidaysFromDb = await _referenceDataReader.GetReferenceData<BankHolidays>();

            if (bankHolidaysFromDb == null)
                return true;

            var bankHolidaysFromApiJson = JsonConvert.SerializeObject(bankHolidaysFromApi.Data);
            var bankHolidaysFromDbJson = JsonConvert.SerializeObject(bankHolidaysFromDb.Data);
            var areEqual = JToken.DeepEquals(bankHolidaysFromApiJson, bankHolidaysFromDbJson);
            return !areEqual;            
        }
    }

}
