﻿using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Domain.Repositories
{
    public interface IVacancyRepository
    {
        Task CreateAsync(Vacancy vacancy);
        Task UpdateAsync(Vacancy vacancy);
        Task<Vacancy> GetVacancyAsync(Guid id);
        Task<IEnumerable<Vacancy>> GetVacanciesByEmployerAccountAsync(string employerAccountId);
    }
}