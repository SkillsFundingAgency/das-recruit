using System;
using System.Threading.Tasks;
using Esfa.Recruit.Storage.Client.Core.Entities;
using Esfa.Recruit.Storage.Client.Core.Entities.VacancyPatches;

namespace Esfa.Recruit.Storage.Client.Core.Repositories
{
    public class StubVacancyRepository : ICommandVacancyRepository, IQueryVacancyRepository
    {

        private static Vacancy _vacancy;
        
        public Task<Vacancy> GetVacancyAsync(Guid vacancyId)
        {
            return Task.FromResult(_vacancy);   
        }
        
        public async Task UpsertVacancyAsync(Guid vacancyId, IVacancyPatch patch)
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
            
        }
    }
}
