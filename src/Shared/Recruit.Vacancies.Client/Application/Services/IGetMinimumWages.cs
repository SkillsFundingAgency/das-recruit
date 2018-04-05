using System;

namespace Esfa.Recruit.Vacancies.Client.Application.Services
{
    public interface IGetMinimumWages
    {
        decimal GetMinimumWage(DateTime date);
    }

}
