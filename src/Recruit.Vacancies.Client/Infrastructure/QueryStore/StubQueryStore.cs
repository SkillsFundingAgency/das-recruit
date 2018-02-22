using Esfa.Recruit.Storage.Client.Domain.Entities;
using Esfa.Recruit.Storage.Client.Domain.QueryStore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recruit.Vacancies.Client.Infrastructure.QueryStore
{
    internal sealed class StubQueryStore : IQueryStoreReader, IQueryStoreWriter
    {
        public Task<IEnumerable<Vacancy>> GetVacanciesAsync(string employerAccountId)
        {
            var vacancies = new List<Vacancy>
            {
                new Vacancy
                {
                    Title = "Ozzy Scott"
                }
            };
            return Task.FromResult(vacancies.AsEnumerable());
        }
    }
}