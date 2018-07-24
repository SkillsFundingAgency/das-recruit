using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.MinimumWage
{
    public interface IGetMinimumWages
    {
        decimal GetApprenticeNationalMinimumWage(DateTime date);

        WageRange GetNationalMinimumWageRange(DateTime date);
    }

}
