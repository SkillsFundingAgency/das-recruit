using System;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Services.ReferenceData;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.BankHolidays;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestSharp;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services
{
    public class BankHolidayUpdateService : IBankHolidayUpdateService
    {
        private readonly BankHolidayConfiguration _config;
        private readonly IReferenceDataWriter _referenceDataWriter;
        private readonly ILogger<BankHolidayUpdateService> _logger;

        public BankHolidayUpdateService(IOptions<BankHolidayConfiguration> config, IReferenceDataWriter referenceDataWriter, ILogger<BankHolidayUpdateService> logger)
        {
            _config = config.Value;
            _referenceDataWriter = referenceDataWriter;
            _logger = logger;
        }

        public async Task UpdateBankHolidaysAsync()
        {
            var client = new RestClient(_config.Url);
            var request = new RestRequest();
            var response = await client.ExecuteTaskAsync<BankHolidays.BankHolidaysData>(request);

            if(!response.IsSuccessful)
                throw new Exception($"Error getting list of bank holidays from url:{_config.Url}. Error:{response.ErrorMessage}");

            if (!response.Data.EnglandAndWales.Events.Any())
                throw new Exception($"Expected a non-empty list of bank holidays from url:{_config.Url}");

            var bankHolidays = new BankHolidays
            {
                Data = response.Data
            };

            await _referenceDataWriter.UpsertReferenceData(bankHolidays);

            _logger.LogInformation($"Upserted bank holidays into ReferenceData store. Last England and Wales date:{bankHolidays.Data.EnglandAndWales.Events.Last().Date}");
        }
    }
}
