using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Services.ReferenceData;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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

            if(!response.IsSuccessful)
                throw new Exception($"Error getting list of bank holidays from url:{_config.Url}. Error:{response.ErrorMessage}");

            if (!response.Data.EnglandAndWales.Events.Any())
                throw new Exception($"Expected a non-empty list of bank holidays from url:{_config.Url}");

            var bankHolidays = new BankHolidays
            {
                Data = response.Data
            };

            await ValidateBankHolidayData(bankHolidays);

            await _referenceDataWriter.UpsertReferenceData(bankHolidays);

            _logger.LogInformation($"Upserted bank holidays into ReferenceData store. Last England and Wales date:{bankHolidays.Data.EnglandAndWales.Events.Last().Date}");
        }

        private async Task ValidateBankHolidayData(BankHolidays bankHolidays)
        {
            var existingBankHolidays = await _bankholidayProvider.GetBankHolidayListAsync();
            var apiCount = bankHolidays.Data.EnglandAndWales.Events.Count;
            var dbCount = existingBankHolidays.Data.EnglandAndWales.Events.Count;

            List<BankHolidays.Event> apiDocuments = bankHolidays.Data.EnglandAndWales.Events;
            List<BankHolidays.Event> dbDocuments = existingBankHolidays.Data.EnglandAndWales.Events;

            

            var test1 = new List<BankHolidays.Event>();
            var test2 = new List<BankHolidays.Event>();

            test1.Add(new BankHolidays.Event() {
                Bunting = true,
                Date = "2012-01-02",
                Notes="",
                Title="Title1"
            });

            test1.Add(new BankHolidays.Event() {
                Bunting = true,
                Date = "2012-01-02",
                Notes = "",
                Title = "Title2"
            });

            test1.Add(new BankHolidays.Event() {
                Bunting = true,
                Date = "2012-01-24",
                Notes = "",
                Title = "Title3"
            });

            test2.Add(new BankHolidays.Event() {
                Bunting = true,
                Date = "2012-01-02",
                Notes = "",
                Title = "Title1"
            });

            test2.Add(new BankHolidays.Event() {
                Bunting = true,
                Date = "2012-01-02",
                Notes = "",
                Title = "Title2"
            });

            test2.Add(new BankHolidays.Event() {
                Bunting = true,
                Date = "2012-01-23",
                Notes = "",
                Title = "Title4"
            });

            foreach (var holiday in apiDocuments)
            {
                if (!dbDocuments.Contains(holidy))
                {
                    var test = holiday;
                }
            }

            var difList = apiDocuments.Where(item => dbDocuments.Select(item2 => item2).Contains(item));

            var difList2 = test1.Where(item => test2.Select(item2 => item2).Contains(item));
            var test4 = test1.Except(test2);
            var test3 = test2.Except(test1);

            var result = test1.Concat(test2) //concatenate list into single recordset
                .Except(test1.Intersect(test2)) //eliminate common items
                .Select(a => new
                {
                    Value = a, //get value
                    List = test1.Any(c => c == a) ? "Test1" : "Test2" //define list
                });

            foreach (var d in result)
            {
                Console.WriteLine("Item: '{0}' found on: '{1}'", d.Value, d.List);
            }

            IEnumerable<BankHolidays.Event> differenceQuery = apiDocuments.Except(dbDocuments);

            var difference = Math.Abs(apiCount - dbCount);
            if (difference > 0)
            {
                _logger.LogCritical("There is a mismatch with between the data of BankHolidays");
            }

            if (difList.Any())
            {
                _logger.LogCritical("There is a mismatch with between the data of BankHolidays");
            }            
        }
    }
}
