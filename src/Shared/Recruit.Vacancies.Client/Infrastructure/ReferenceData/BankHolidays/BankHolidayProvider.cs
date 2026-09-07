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
    public class BankHolidayProvider(IOuterApiClient outerApiClient, ICache cache, ITimeProvider timeProvider)
        : IBankHolidayProvider
    {
        public async Task<List<DateTime>> GetBankHolidaysAsync()
        {
            return await cache.CacheAsideAsync(CacheKeys.BankHolidays,
                timeProvider.NextDay,
                async () =>
                {
                    var bankHolidayReferenceData = await outerApiClient.Get<BankHolidaysData>(new GetBankHolidaysRequest());

                    return bankHolidayReferenceData.EnglandAndWales.Events
                        .Select(e => DateTime.Parse(e.Date))
                        .ToList();
                });
        }        
    }
}
