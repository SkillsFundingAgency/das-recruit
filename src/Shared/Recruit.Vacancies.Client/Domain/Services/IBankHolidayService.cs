using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Domain.Services
{
    public interface IBankHolidayService
    {
        Task<List<DateTime>> GetBankHolidaysAsync();

        Task UpdateBankHolidaysAsync();
    }
}
