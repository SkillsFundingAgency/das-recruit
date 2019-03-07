using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Cache;
using Esfa.Recruit.Vacancies.Client.Application.Providers;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.BankHolidays
{
    public class BankHolidayProvider : IBankHolidayProvider
    {
        private readonly IReferenceDataReader _referenceDataReader;
        private readonly ICache _cache;
        private readonly ITimeProvider _timeProvider;

        public BankHolidayProvider(IReferenceDataReader referenceDataReader, ICache cache, ITimeProvider timeProvider)
        {
            _referenceDataReader = referenceDataReader;
            _cache = cache;
            _timeProvider = timeProvider;
        }

        public async Task<List<DateTime>> GetBankHolidaysAsync()
        {
            return await _cache.CacheAsideAsync(CacheKeys.BankHolidays,
                _timeProvider.NextDay,
                async () =>
                {
                    var bankHolidayReferenceData = await _referenceDataReader.GetReferenceData<BankHolidays>();

                    return bankHolidayReferenceData.Data.EnglandAndWales.Events
                        .Select(e => DateTime.Parse(e.Date))
                        .ToList();
                });
        }        
    }
}
