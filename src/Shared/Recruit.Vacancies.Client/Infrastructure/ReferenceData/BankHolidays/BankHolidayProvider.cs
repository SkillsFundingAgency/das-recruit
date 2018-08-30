using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.BankHolidays;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services
{
    public class BankHolidayProvider : IBankHolidayProvider
    {
        private readonly IReferenceDataReader _referenceDataReader;

        public BankHolidayProvider(IReferenceDataReader referenceDataReader)
        {
            _referenceDataReader = referenceDataReader;
        }

        public async Task<List<DateTime>> GetBankHolidaysAsync()
        {
            var bankHolidayReferenceData = await _referenceDataReader.GetReferenceData<BankHolidays>();

            return bankHolidayReferenceData.Data.EnglandAndWales.Events
                .Select(e => DateTime.Parse(e.Date))
                .ToList();
        }
    }
}
