using System;
using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.Extensions.Logging;
using MinWageEntity = Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.Wages.MinimumWage;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.Wages
{
    public class NationalMinimumWageProvider : IMinimumWageProvider
    {
        private readonly ILogger<NationalMinimumWageProvider> _logger;
        private readonly Lazy<IList<MinWageEntity>> _wagePeriods;
        
        public NationalMinimumWageProvider(IReferenceDataReader referenceDataReader, ILogger<NationalMinimumWageProvider> logger)
        {
            _logger = logger;
            _wagePeriods = new Lazy<IList<MinWageEntity>>(() => referenceDataReader.GetReferenceData<MinimumWages>().Result.Ranges
                .OrderBy(w => w.ValidFrom).ToList());
        }   

        public decimal GetApprenticeNationalMinimumWage(DateTime date)
        {
            var matchingPeriod = GetWagePeriod(date); 
            
            return matchingPeriod.ApprenticeshipMinimumWage;
        }

        public WageRange GetNationalMinimumWageRange(DateTime date)
        {
            var matchingPeriod = GetWagePeriod(date); 

            return new WageRange { MinimumWage = matchingPeriod.NationalMinimumWageLowerBound, MaximumWage = matchingPeriod.NationalMinimumWageUpperBound };
        }

        private MinWageEntity GetWagePeriod(DateTime date)
        {
            try
            {
                var wagePeriods = _wagePeriods.Value;

                MinWageEntity currentWagePeriod = null;
                foreach (var wagePeriod in wagePeriods)
                {
                    if (date.Date < wagePeriod.ValidFrom)
                        break;

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
    }
}

