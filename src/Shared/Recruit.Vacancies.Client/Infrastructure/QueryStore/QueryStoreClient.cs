using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Projections;
using Esfa.Recruit.Vacancies.Client.Domain.QueryStore;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore
{
    public class QueryStoreClient : IQueryStoreReader, IQueryStoreWriter
    {
        private readonly IQueryStore _queryStore;

        public QueryStoreClient(IQueryStore queryStore)
        {
            _queryStore = queryStore;
        }

        public Task<Dashboard> GetDashboardAsync(string employerAccountId)
        {
            var key = string.Format(QueryViewKeys.DashboardViewPrefix, employerAccountId);

            return _queryStore.GetAsync<Dashboard>(key);
        }

        public Task UpdateDashboardAsync(string employerAccountId, IEnumerable<VacancySummary> vacancySummaries)
        {
            var dashboard = new Dashboard
            {
                Id = string.Format(QueryViewKeys.DashboardViewPrefix, employerAccountId),
                Vacancies = vacancySummaries
            };

            return _queryStore.UpdsertAsync<Dashboard>(dashboard);
        }

        public Task UpdateApprenticeshipProgrammesAsync(ApprenticeshipProgrammes programmes)
        {
            return _queryStore.UpdsertAsync<ApprenticeshipProgrammes>(programmes);
        }
    }
}
