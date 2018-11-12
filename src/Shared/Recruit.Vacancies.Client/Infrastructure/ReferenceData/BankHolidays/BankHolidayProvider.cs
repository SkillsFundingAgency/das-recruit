using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Cache;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.BankHolidays;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services
{
    public class BankHolidayProvider : IBankHolidayProvider
    {
        private readonly IReferenceDataReader _referenceDataReader;
        private readonly ICache _cache;

        private DateTime CacheAbsoluteExpiryTime => DateTime.UtcNow.Date.AddDays(1);

        public BankHolidayProvider(IReferenceDataReader referenceDataReader, ICache cache)
        {
            _referenceDataReader = referenceDataReader;
            _cache = cache;
        }

        public async Task<List<DateTime>> GetBankHolidaysAsync()
        {
            return await _cache.CacheAsideAsync(CacheKeys.BankHolidays,
                CacheAbsoluteExpiryTime,
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
