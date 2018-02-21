using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Storage.Client.Core.Entities;

namespace Esfa.Recruit.Storage.Client.Core.Repositories
{
    public interface IQueryVacancyRepository
    {
        Task<Vacancy> GetVacancyAsync(Guid vacancyId);
        Task<IEnumerable<Vacancy>> GetVacanciesAsync(string employerAccountId);
    }
}