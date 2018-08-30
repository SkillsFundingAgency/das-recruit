using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Application.Providers
{
    public interface IMinimumWageProvider
    {
        decimal GetApprenticeNationalMinimumWage(DateTime date);

        WageRange GetNationalMinimumWageRange(DateTime date);
    }

}
