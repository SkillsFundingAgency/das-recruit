using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.QueryStore;
using Esfa.Recruit.Vacancies.Client.Domain.Projections;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.Services
{
    public class DashboardService : ICreateDashboards
    {
        private readonly IQueryStoreWriter _queryStore;
        private readonly IVacancyRepository _repository;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(IQueryStoreWriter queryStore, IVacancyRepository repository, ILogger<DashboardService> logger)
        {
            _logger = logger;
            _repository = repository;
            _queryStore = queryStore;
        }

        public async Task BuildDashboard(string employerAccountId)
        {
            var vacancySummaries = await _repository.GetVacanciesByEmployerAccountAsync<VacancySummary>(employerAccountId);

            var activeVacancySummaries = vacancySummaries.Where(v => v.IsDeleted == false);

            await _queryStore.UpdateDashboardAsync(employerAccountId, activeVacancySummaries.OrderBy(v => v.CreatedDate));
            
            _logger.LogDebug("Update dashboard with {count} summary records for account: {employerAccountId}", activeVacancySummaries.Count(), employerAccountId);
        }
    }
}