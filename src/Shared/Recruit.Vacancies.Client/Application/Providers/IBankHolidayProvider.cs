using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.BankHolidays;

namespace Esfa.Recruit.Vacancies.Client.Application.Providers
{
    public interface IBankHolidayProvider
    {
        Task<List<DateTime>> GetBankHolidaysAsync();        
    }
}