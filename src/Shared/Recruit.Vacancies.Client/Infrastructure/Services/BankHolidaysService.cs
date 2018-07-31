using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Services;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services
{
    public class BankHolidaysService : IBankHolidayService
    {
        private readonly IReferenceDataReader _referenceData;

        public BankHolidaysService(IReferenceDataReader referenceData)
        {
            _referenceData = referenceData;
        }

        public async Task<List<DateTime>> GetBankHolidaysAsync()
        {
            var bankHolidayReferenceData = await _referenceData.GetBankHolidaysAsync();

            return bankHolidayReferenceData.Data.EnglandAndWales.Events
                .Select(e => DateTime.Parse(e.Date))
                .ToList();
        }
    }
}
