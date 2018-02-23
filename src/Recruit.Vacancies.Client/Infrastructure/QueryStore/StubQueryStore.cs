using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.QueryStore;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore
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