using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.VacancySummariesProvider;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Projections
{
    internal class EmployerDashboardProjectionService : IEmployerDashboardProjectionService
    {
        private readonly ILogger<EmployerDashboardProjectionService> _logger;
        private readonly IVacancyQuery _vacancyQuery;
        private readonly IVacancySummariesProvider _vacancySummariesQuery;
        private readonly IQueryStoreWriter _queryStoreWriter;
        private readonly IApprenticeshipProgrammeProvider _apprenticeshipProgrammeProvider;
        private readonly ITimeProvider _timeProvider;

        public EmployerDashboardProjectionService(
            IVacancyQuery vacancyQuery,
            IVacancySummariesProvider vacancySummariesQuery,
            IQueryStoreWriter queryStoreWriter,
            ILogger<EmployerDashboardProjectionService> logger,
            IApprenticeshipProgrammeProvider apprenticeshipProgrammeProvider,
            ITimeProvider timeProvider)
        {
            _logger = logger;
            _vacancyQuery = vacancyQuery;
            _queryStoreWriter = queryStoreWriter;
            _vacancySummariesQuery = vacancySummariesQuery;
            _apprenticeshipProgrammeProvider = apprenticeshipProgrammeProvider;
            _timeProvider = timeProvider;
        }

        public async Task ReBuildAllDashboardsAsync()
        {
            var employerAccountIds = (await _vacancyQuery.GetDistinctVacancyOwningEmployerAccountsAsync()).ToList();

            _logger.LogInformation($"Rebuilding {employerAccountIds.Count} employer dashboards");

            var startDateTime = _timeProvider.Now;
            var stopwatch = Stopwatch.StartNew();

            foreach (var id in employerAccountIds)
            {
                await ReBuildDashboardAsync(id);
            }

            _logger.LogInformation($"Rebuilding {employerAccountIds.Count} employer dashboards took {stopwatch.ElapsedMilliseconds} milliseconds");

            var count = await _queryStoreWriter.RemoveOldEmployerDashboards(startDateTime);

            _logger.LogInformation($"Removed {count} redundant/old employer dashboards from query store");
        }

        public async Task ReBuildDashboardAsync(string employerAccountId)
        {
            var vacancySummaries = await _vacancySummariesQuery.GetEmployerOwnedVacancySummariesByEmployerAccountAsync(employerAccountId);
            var reviewSummaries = await _vacancySummariesQuery.GetProviderOwnedVacancySummariesInReviewByEmployerAccountAsync(employerAccountId);

            var summaries = vacancySummaries.Concat(reviewSummaries).ToList();

            foreach (var summary in summaries)
            {
                await UpdateWithTrainingProgrammeInfo(summary);
            }

            await _queryStoreWriter.UpdateEmployerDashboardAsync(employerAccountId, summaries.OrderBy(v => v.CreatedDate));

            _logger.LogDebug("Update employer dashboard with {count} summary records for account: {employerAccountId}", summaries.Count, employerAccountId);
        }

        private async Task UpdateWithTrainingProgrammeInfo(VacancySummary summary)
        {
            if (summary.ProgrammeId != null)
            {
                var programme = await _apprenticeshipProgrammeProvider.GetApprenticeshipProgrammeAsync(summary.ProgrammeId);

                if (programme == null)
                {
                    _logger.LogWarning($"No training programme found for ProgrammeId: {summary.ProgrammeId}");
                }
                else
                {
                    summary.TrainingTitle = programme.Title;
                    summary.TrainingType = programme.ApprenticeshipType;
                    summary.TrainingLevel = programme.ApprenticeshipLevel;
                }
            }
        }
    }
}
