using System;
using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.Entities;
using Microsoft.Extensions.Logging;
using MinWageEntity = Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.Entities.MinimumWage;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Wages
{
    public class NationalMinimumWageService : IGetMinimumWages
    {
        private readonly ILogger<NationalMinimumWageService> _logger;
        private readonly Lazy<IList<MinWageEntity>> _wagePeriods;
        
        public NationalMinimumWageService(IReferenceDataReader referenceDataReader, ILogger<NationalMinimumWageService> logger)
        {
            _logger = logger;
            _wagePeriods = new Lazy<IList<MinWageEntity>>(() => referenceDataReader.GetReferenceData<MinimumWages>().Result.Ranges);
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
                return _wagePeriods.Value.Single(x => date.Date >= x.ValidFrom && date.Date <= x.ValidTo);
            }
            catch(InvalidOperationException ex)
            {
                _logger.LogError(ex, $"Unable to find Wage Period for date: {date}");
                
                throw;
            }
        }
    }
}

