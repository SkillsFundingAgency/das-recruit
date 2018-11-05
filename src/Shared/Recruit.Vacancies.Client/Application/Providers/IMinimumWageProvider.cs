﻿using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Application.Providers
{
    public interface IMinimumWageProvider
    {
        IMinimumWage GetWagePeriod(DateTime date);
    }

}
