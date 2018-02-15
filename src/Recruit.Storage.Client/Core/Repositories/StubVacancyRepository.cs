using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Esfa.Recruit.Storage.Client.Core.Entities;

namespace Esfa.Recruit.Storage.Client.Core.Repositories
{
    public class StubRepository : IVacancyRepository
    {

        private static Vacancy _vacancy;

        public Task CreateVacancyAsync(Vacancy vacancy)
        {
            _vacancy = vacancy;
            return Task.CompletedTask;
        }

        public Task<Vacancy> GetVacancyAsync(Guid id)
        {
            if(_vacancy.Id == id)
            {
                return Task.FromResult(_vacancy);
            }
            
            return null;
        }
        
        public Task UpdateVacancyAsync(Vacancy vacancy)
        {
            if(vacancy.Id == vacancy.Id)
            {
                _vacancy = vacancy;
            }
            return Task.CompletedTask;
        }
    }
}
