using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Dashboard;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services
{
    internal class DashboardService : IDashboardService
    {
        private readonly ILogger<DashboardService> _logger;
        private readonly IVacancyRepository _repository;
        private readonly IQueryStoreWriter _queryStoreWriter;

        public DashboardService(IVacancyRepository repository, IQueryStoreWriter queryStoreWriter, ILogger<DashboardService> logger)
        {
            _logger = logger;
            _repository = repository;
            _queryStoreWriter = queryStoreWriter;
        }

        public async Task ReBuildDashboardAsync(string employerAccountId)
        {
            var vacancySummaries = await _repository.GetVacanciesByEmployerAccountAsync<VacancySummary>(employerAccountId);

            var activeVacancySummaries = vacancySummaries.Where(v => v.IsDeleted == false).ToList();

            foreach (var summary in activeVacancySummaries)
            {
                // PendingReview shows as Submitted in Dashboard.
                if (summary.Status == VacancyStatus.PendingReview)
                {
                    summary.Status = VacancyStatus.Submitted;
                }
            }

            await _queryStoreWriter.UpdateDashboardAsync(employerAccountId, activeVacancySummaries.OrderBy(v => v.CreatedDate));

            _logger.LogDebug("Update dashboard with {count} summary records for account: {employerAccountId}", activeVacancySummaries.Count, employerAccountId);
        }
    }
}
