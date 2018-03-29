using System;

namespace Esfa.Recruit.Vacancies.Client.Application.Services
{
    public interface IGetApprenticeNationalMinimumWages
    {
        decimal GetMinimumWage(DateTime date);
    }

}
