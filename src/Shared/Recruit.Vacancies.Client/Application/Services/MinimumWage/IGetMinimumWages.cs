using System;

namespace Esfa.Recruit.Vacancies.Client.Application.Services.MinimumWage
{
    public interface IGetMinimumWages
    {
        decimal GetApprenticeNationalMinimumWage(DateTime date);

        WageRange GetNationalMinimumWageRange(DateTime date);
    }

}
