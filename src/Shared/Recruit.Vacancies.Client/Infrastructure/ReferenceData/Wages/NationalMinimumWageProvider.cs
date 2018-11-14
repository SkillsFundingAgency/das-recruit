using System;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Cache;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.Extensions.Logging;
using MinWageEntity = Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.Wages.MinimumWage;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.Wages
{
    public class NationalMinimumWageProvider : IMinimumWageProvider
    {
        private readonly IReferenceDataReader _referenceDataReader;
        private readonly ILogger<NationalMinimumWageProvider> _logger;
        private readonly ICache _cache;
        private readonly ITimeProvider _timeProvider;

        public NationalMinimumWageProvider(IReferenceDataReader referenceDataReader, ILogger<NationalMinimumWageProvider> logger, ICache cache, ITimeProvider timeProvider)
        {
            _referenceDataReader = referenceDataReader;
            _logger = logger;
            _cache = cache;
            _timeProvider = timeProvider;
        }   

        public IMinimumWage GetWagePeriod(DateTime date)
        {
            try
            {
                var minimumWages = GetMinimumWagesAsync().Result;

                var wagePeriods = minimumWages.Ranges.OrderBy(w => w.ValidFrom).ToList();

                MinWageEntity currentWagePeriod = null;
                foreach (var wagePeriod in wagePeriods)
                {
                    if (date.Date < wagePeriod.ValidFrom)
                        break;

                    if (currentWagePeriod != null && currentWagePeriod.ValidFrom == wagePeriod.ValidFrom)
                        throw new InvalidOperationException($"Duplicate wage period: {currentWagePeriod.ValidFrom}");

                    currentWagePeriod = wagePeriod;
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

        private Task<MinimumWages> GetMinimumWagesAsync()
        {
            return _cache.CacheAsideAsync(CacheKeys.MinimumWages,
                _timeProvider.NextDay,
                () => _referenceDataReader.GetReferenceData<MinimumWages>());
        }
    }
}

