using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Storage.Client.Core.Entities;
using Esfa.Recruit.Storage.Client.Core.Entities.VacancyPatches;
using System.Linq;

namespace Esfa.Recruit.Storage.Client.Core.Repositories
{
    public class StubVacancyRepository : ICommandVacancyRepository, IQueryVacancyRepository
    {
        private static Vacancy _vacancy;
        
        public Task<Vacancy> GetVacancyAsync(Guid vacancyId)
        {
            return Task.FromResult(_vacancy);
        }

        public Task<IEnumerable<Vacancy>> GetVacanciesAsync(string employerAccountId)
        {
            var vacancies = new List<Vacancy>
            {
                _vacancy
            };
            return Task.FromResult(vacancies.AsEnumerable());
        }

        public Task UpsertVacancyAsync(Guid vacancyId, IVacancyPatch patch)
        {
            if(_vacancy == null)
            {
                _vacancy = new Vacancy
                {
                    Id = vacancyId
                };
            }

            foreach(var property in patch.GetType().GetProperties())
            {
                typeof(Vacancy).GetProperty(property.Name).SetValue(_vacancy, property.GetValue(patch));
            }

            return Task.CompletedTask;
        }
    }
}