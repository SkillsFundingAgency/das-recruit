using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.QueryStore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore
{
    internal sealed class StubQueryStore : IQueryStoreReader, IQueryStoreWriter
    {
        public Task<Dashboard> GetDashboardAsync(string employerAccountId)
        {
            var dashboard = new Dashboard
            {
                EmployerAccountId = employerAccountId,
                Vacancies = new List<VacancySummary>
                {
                    new VacancySummary
                    {
                        Title = "Ozzy Scott"
                    }
                }
            };
            return Task.FromResult(dashboard);
        }

        public Task UpdateDashboardAsync(string key, Dashboard dashboard)
        {
            return Task.CompletedTask;
        }
    }
}