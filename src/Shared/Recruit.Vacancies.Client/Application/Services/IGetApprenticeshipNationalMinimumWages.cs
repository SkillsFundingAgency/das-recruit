using System;

namespace Esfa.Recruit.Vacancies.Client.Application.Services
{
    public interface IGetApprenticeshipNationalMinimumWages
    {
        decimal GetMinimumWage(DateTime date);
    }

}
