using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Repositories
{
    internal sealed class StubVacancyRepository : IVacancyRepository
    {
        private readonly Dictionary<Guid, Vacancy> _vacancies = new Dictionary<Guid, Vacancy>(50);

        public Task CreateAsync(Vacancy vacancy)
        {
            _vacancies.Add(vacancy.Id, vacancy);

            return Task.CompletedTask;
        }

        public Task<Vacancy> GetVacancyAsync(Guid id)
        {
            var vacancy = _vacancies.Where(kv => kv.Key == id && kv.Value.IsDeleted == false)
                .Select(kv => kv.Value)
                .SingleOrDefault();
            
            return Task.FromResult(vacancy);
        }

        public Task<IEnumerable<Vacancy>> GetVacanciesByEmployerAccountAsync(string employerAccountId)
        {
            var vacancies = _vacancies.Where(kv => kv.Value.EmployerAccountId == employerAccountId && kv.Value.IsDeleted == false)
                                        .Select(kv => kv.Value);

            return Task.FromResult(vacancies);
        }

        public Task UpdateAsync(Vacancy vacancy)
        {
            _vacancies[vacancy.Id] = vacancy;

            return Task.CompletedTask;
        }
    }
}