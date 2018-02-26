using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Repositories
{
    internal sealed class StubVacancyRepository : IVacancyRepository
    {

        private Dictionary<Guid, Vacancy> _vacancies = new Dictionary<Guid, Vacancy>(50);

        public Task CreateAsync(Vacancy vacancy)
        {
            _vacancies.Add(vacancy.Id, vacancy);

            return Task.CompletedTask;
        }

        public Task<Vacancy> GetVacancyAsync(Guid id)
        {
            return Task.FromResult(_vacancies[id]);
        }

        public Task<Vacancy> GetVacancyForEditAsync(Guid vacancyId)
        {
            return Task.FromResult(_vacancies[vacancyId]);   
        }

        public Task UpdateAsync(Vacancy vacancy)
        {
            _vacancies[vacancy.Id] = vacancy;

            return Task.CompletedTask;
        }
    }
}
