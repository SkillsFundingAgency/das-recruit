using System;

namespace Esfa.Recruit.Vacancies.Client.Application.Services
{
    public interface IGetMinimumWages
    {
        decimal GetApprenticeNationalMinimumWage(DateTime date);
    }

}
