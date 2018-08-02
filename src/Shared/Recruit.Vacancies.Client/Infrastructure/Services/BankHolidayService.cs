using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestSharp;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services
{
    public class BankHolidayService : IBankHolidayService
    {
        private readonly BankHolidayConfiguration _config;
        private readonly IReferenceDataReader _referenceDataReader;
        private readonly IReferenceDataWriter _referenceDataWriter;
        private readonly ILogger<BankHolidayService> _logger;

        public BankHolidayService(IOptions<BankHolidayConfiguration> config, IReferenceDataReader referenceDataReader, IReferenceDataWriter referenceDataWriter, ILogger<BankHolidayService> logger)
        {
            _config = config.Value;
            _referenceDataReader = referenceDataReader;
            _referenceDataWriter = referenceDataWriter;
            _logger = logger;
        }

        public async Task<List<DateTime>> GetBankHolidaysAsync()
        {
            var bankHolidayReferenceData = await _referenceDataReader.GetBankHolidaysAsync();

            return bankHolidayReferenceData.Data.EnglandAndWales.Events
                .Select(e => DateTime.Parse(e.Date))
                .ToList();
        }

        public async Task UpdateBankHolidaysAsync()
        {
            var client = new RestClient(_config.Url);
            var request = new RestRequest();
            var response = await client.ExecuteTaskAsync<BankHolidays.BankHolidaysData>(request);

            if(!response.Data.EnglandAndWales.Events.Any())
                throw new Exception($"Expected a list of bank holidays from url:{_config.Url}");

            var bankHolidays = new BankHolidays
                {Data = response.Data};

            await _referenceDataWriter.UpsertBankHolidays(bankHolidays);

            _logger.LogInformation($"Upserted bank holidays into ReferenceData store. Last England and Wales date:{bankHolidays.Data.EnglandAndWales.Events.Last().Date}");
        }
    }
}
