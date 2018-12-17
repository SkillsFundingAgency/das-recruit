using System;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Cache;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.Extensions.Logging;
using MinWageEntity = Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.Wages.MinimumWage;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.Wages
{
    using System.Collections.Immutable;
    using SFA.DAS.VacancyServices.Wage;

    public class NationalMinimumWageProvider : IMinimumWageProvider
    {
        private readonly ILogger<NationalMinimumWageProvider> _logger;
        private readonly ICache _cache;
        private readonly ITimeProvider _timeProvider;

        public NationalMinimumWageProvider(ILogger<NationalMinimumWageProvider> logger, ICache cache, ITimeProvider timeProvider)
        {
            _logger = logger;
            _cache = cache;
            _timeProvider = timeProvider;
        }   

        public IMinimumWage GetWagePeriod(DateTime date)
        {
            try
            {
                var minimumWages = GetMinimumWagesAsync().Result;

                var wagePeriods = minimumWages.OrderBy(w => w.ValidFrom);

                MinWageEntity currentWagePeriod = null;
                foreach (var wagePeriod in wagePeriods)
                {
                    if (date.Date < wagePeriod.ValidFrom)
                        break;

                    if (currentWagePeriod != null && currentWagePeriod.ValidFrom == wagePeriod.ValidFrom)
                        throw new InvalidOperationException($"Duplicate wage period: {currentWagePeriod.ValidFrom}");

                    currentWagePeriod = new MinWageEntity
                    {
                        ValidFrom = wagePeriod.ValidFrom,
                        ApprenticeshipMinimumWage = wagePeriod.ApprenticeMinimumWage,
                        NationalMinimumWageLowerBound = wagePeriod.Under18NationalMinimumWage,
                        NationalMinimumWageUpperBound = wagePeriod.Over25NationalMinimumWage
                    };
                }

                if (currentWagePeriod == null)
                    throw new InvalidOperationException("Wage period is missing");

                return currentWagePeriod;
            }
            catch(InvalidOperationException ex)
            {
                _logger.LogError(ex, $"Unable to find Wage Period for date: {date}");
                
                throw;
            }
        }

        private Task<ImmutableArray<NationalMinimumWageRates>> GetMinimumWagesAsync()
        {
            return _cache.CacheAsideAsync(CacheKeys.MinimumWages,
                _timeProvider.NextDay,
                NationalMinimumWageService.GetRatesAsync);
        }
    }
}

