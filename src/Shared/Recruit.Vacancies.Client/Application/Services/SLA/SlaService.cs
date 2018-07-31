using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Services;
using Microsoft.IdentityModel.Tokens;

namespace Esfa.Recruit.Vacancies.Client.Application.Services.SLA
{
    public class SlaService : ISlaService
    {
        private const int SlaHours = 24;

        private readonly IBankHolidayService _bankholidayService;

        public SlaService(IBankHolidayService bankholidayService)
        {
            _bankholidayService = bankholidayService;
        }

        public async Task<DateTime> GetSlaDeadlineAsync(DateTime utcDate)
        {
            //https://www.gov.uk/bank-holidays.json
            var bankHolidays = await _bankholidayService.GetBankHolidaysAsync();

            var slaDate = utcDate;

            //Handle vacancies submitted during non-working days
            while (ValidateSlaDate(slaDate, bankHolidays) == false)
            {
                slaDate = slaDate.Date.AddDays(1);
            }

            //add the SLA duration
            slaDate = slaDate.AddHours(SlaHours);

            //find the next working day
            while(ValidateSlaDate(slaDate, bankHolidays) == false)
            {
                slaDate = slaDate.AddDays(1);
            }

            return slaDate;
        }

        private bool ValidateSlaDate(DateTime slaDate, IEnumerable<DateTime> bankHolidays)
        {
            if (slaDate.DayOfWeek == DayOfWeek.Saturday || slaDate.DayOfWeek == DayOfWeek.Sunday)
            {
                return false;
            }

            if (bankHolidays.Contains(slaDate.Date))
            {
                return false;
            }

            return true;
        }
    }
}
