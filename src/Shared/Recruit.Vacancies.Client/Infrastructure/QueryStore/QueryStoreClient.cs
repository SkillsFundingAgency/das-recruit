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

        public Task<Dashboard> GetDashboardAsync(string viewKey)
        {
            return _queryStore.GetAsync<Dashboard>(viewKey);
        }

        public Task UpdateDashboardAsync(Dashboard dashboard)
        {
            return _queryStore.UpdsertAsync<Dashboard>(dashboard);
        }

        public Task UpdateApprenticeshipProgrammesAsync(ApprenticeshipProgrammes programmes)
        {
            return _queryStore.UpdsertAsync<ApprenticeshipProgrammes>(programmes);
        }
    }
}
