using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.ApplicationReview;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Employer;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Projections
{
    internal class EmployerDashboardProjectionService : IEmployerDashboardProjectionService
    {
        private readonly ILogger<EmployerDashboardProjectionService> _logger;
        private readonly IVacancyRepository _repository;
        private readonly IQueryStoreWriter _queryStoreWriter;
        private readonly IApplicationReviewRepository _applicationReviewRepository;

        public EmployerDashboardProjectionService(IVacancyRepository repository, IApplicationReviewRepository applicationReviewRepository, IQueryStoreWriter queryStoreWriter, ILogger<EmployerDashboardProjectionService> logger)
        {
            _logger = logger;
            _repository = repository;
            _queryStoreWriter = queryStoreWriter;
            _applicationReviewRepository = applicationReviewRepository;
        }

        public async Task ReBuildDashboardAsync(string employerAccountId)
        {
            var vacancySummariesTask = _repository.GetVacanciesByEmployerAccountAsync<VacancySummary>(employerAccountId);

            var applicationReviewsTask = _applicationReviewRepository.GetForEmployerAsync<ApplicationReviewSummary>(
                    employerAccountId);

            await Task.WhenAll(vacancySummariesTask, applicationReviewsTask);

            var vacancySummaries = vacancySummariesTask.Result;
            var applicationReviews = applicationReviewsTask.Result;

            var activeVacancySummaries = vacancySummaries.Where(v => v.IsDeleted == false).ToList();

            foreach (var summary in activeVacancySummaries)
            {
                if (summary.VacancyReference.HasValue)
                {
                    var vacancyApplicationReviews = applicationReviews.Where(r => r.VacancyReference == summary.VacancyReference.Value).ToList();
                    summary.AllApplicationsCount = vacancyApplicationReviews.Count;
                    summary.NewApplicationsCount = vacancyApplicationReviews.Count(r => r.Status == ApplicationReviewStatus.New);
                }
            }

            await _queryStoreWriter.UpdateEmployerDashboardAsync(employerAccountId, activeVacancySummaries.OrderBy(v => v.CreatedDate));

            _logger.LogDebug("Update dashboard with {count} summary records for account: {employerAccountId}", activeVacancySummaries.Count, employerAccountId);
        }
    }
}
