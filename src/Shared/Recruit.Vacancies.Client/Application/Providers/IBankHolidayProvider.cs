using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Application.Providers;

public interface IBankHolidayProvider
{
    Task<List<DateTime>> GetBankHolidaysAsync();        
}