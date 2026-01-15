using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Cache;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.BankHolidays
{
    public class BankHolidayProvider : IBankHolidayProvider
    {
        private readonly IOuterApiClient _outerApiClient;
        private readonly ICache _cache;
        private readonly ITimeProvider _timeProvider;

        public BankHolidayProvider(IOuterApiClient outerApiClient, ICache cache, ITimeProvider timeProvider)
        {
            _outerApiClient = outerApiClient;
            _cache = cache;
            _timeProvider = timeProvider;
        }

        public async Task<List<DateTime>> GetBankHolidaysAsync()
        {
            return await _cache.CacheAsideAsync(CacheKeys.BankHolidays,
                _timeProvider.NextDay,
                async () =>
                {
                    var bankHolidayReferenceData = await _outerApiClient.Get<BankHolidaysData>(new GetBankHolidaysRequest());

                    return bankHolidayReferenceData.EnglandAndWales.Events
                        .Select(e => DateTime.Parse(e.Date))
                        .ToList();
                });
        }        
    }
}
