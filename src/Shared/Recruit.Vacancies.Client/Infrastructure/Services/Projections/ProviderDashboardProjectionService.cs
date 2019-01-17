using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Employer;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Projections
{
    internal class ProviderDashboardProjectionService : IProviderDashboardProjectionService
    {
        private readonly ILogger<ProviderDashboardProjectionService> _logger;
        private readonly IVacancyQuery _vacancyQuery;
        private readonly IQueryStoreWriter _queryStoreWriter;
        private readonly IApplicationReviewQuery _applicationReviewQuery;
        private readonly IApprenticeshipProgrammeProvider _apprenticeshipProgrammeProvider;
        private readonly ITimeProvider _timeProvider;

        public ProviderDashboardProjectionService(
            IVacancyQuery vacancyQuery, 
            IApplicationReviewQuery applicationReviewQuery, 
            IQueryStoreWriter queryStoreWriter, 
            ILogger<ProviderDashboardProjectionService> logger,
            IApprenticeshipProgrammeProvider apprenticeshipProgrammeProvider,
            ITimeProvider timeProvider)
        {
            _logger = logger;
            _vacancyQuery = vacancyQuery;
            _queryStoreWriter = queryStoreWriter;
            _applicationReviewQuery = applicationReviewQuery;
            _apprenticeshipProgrammeProvider = apprenticeshipProgrammeProvider;
            _timeProvider = timeProvider;
        }

        public async Task ReBuildAllDashboardsAsync()
        {
            var ukprns = (await _vacancyQuery.GetDistinctVacancyOwningProviderAccountsAsync()).ToList();

            _logger.LogInformation($"Rebuilding {ukprns.Count} provider dashboards");

            var startDateTime = _timeProvider.Now;
            var stopwatch = Stopwatch.StartNew();

            foreach (var ukprn in ukprns)
            {
                await ReBuildDashboardAsync(ukprn);
            }

            _logger.LogInformation($"Rebuilding {ukprns.Count} provider dashboards took {stopwatch.ElapsedMilliseconds} milliseconds");

            var count = await _queryStoreWriter.RemoveOldProviderDashboards(startDateTime);

            _logger.LogInformation($"Removed {count} redundant/old provider dashboards from query store");
        }

        public async Task ReBuildDashboardAsync(long ukprn)
        {
            var vacancySummariesTask = _vacancyQuery.GetVacanciesByProviderAccountAsync<VacancySummary>(ukprn);

            var applicationReviewStatusCountsTask = _applicationReviewQuery.GetStatusCountsForProviderAsync(ukprn);

            await Task.WhenAll(vacancySummariesTask, applicationReviewStatusCountsTask);

            var vacancySummaries = vacancySummariesTask.Result.ToList();
            var applicationReviewStatusCounts = applicationReviewStatusCountsTask.Result;

            foreach (var summary in vacancySummaries)
            {
                if (summary.VacancyReference.HasValue)
                {
                    summary.AllApplicationsCount = applicationReviewStatusCounts
                        .Where(r => r.Id.VacancyReference == summary.VacancyReference.Value)
                        .Sum(r => r.Count);

                    summary.NewApplicationsCount = applicationReviewStatusCounts
                        .Where(r => r.Id.VacancyReference == summary.VacancyReference.Value &&
                                    r.Id.Status == ApplicationReviewStatus.New)
                        .Sum(r => r.Count);
                }

                await UpdateWithTrainingProgrammeInfo(summary);
            }

            await _queryStoreWriter.UpdateProviderDashboardAsync(ukprn, vacancySummaries.OrderBy(v => v.CreatedDate));

            _logger.LogDebug("Update provider dashboard with {count} summary records for account: {ukprn}", vacancySummaries.Count, ukprn);
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
                    summary.TrainingLevel = programme.Level;
                }
            }
        }
    }
}
