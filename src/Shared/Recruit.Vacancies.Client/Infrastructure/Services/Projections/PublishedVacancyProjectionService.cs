using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Extensions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Vacancy;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.ApprenticeshipProgrammes;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Projections
{
    public class PublishedVacancyProjectionService : IPublishedVacancyProjectionService
    {
        private readonly ILogger<PublishedVacancyProjectionService> _logger;
        private readonly IQueryStoreWriter _queryStoreWriter;
        private readonly IVacancyQuery _vacancyQuery;
        private readonly IApprenticeshipProgrammeProvider _apprenticeshipProgrammeProvider;
        private readonly ITimeProvider _timeProvider;

        public PublishedVacancyProjectionService(
                                    ILogger<PublishedVacancyProjectionService> logger, 
                                    IQueryStoreWriter queryStoreWriter, 
                                    IVacancyQuery vacancyQuery,
                                    IApprenticeshipProgrammeProvider apprenticeshipProgrammeProvider,
                                    ITimeProvider timeProvider)
        {
            _logger = logger;
            _queryStoreWriter = queryStoreWriter;
            _vacancyQuery = vacancyQuery;
            _apprenticeshipProgrammeProvider = apprenticeshipProgrammeProvider;
            _timeProvider = timeProvider;
        }

        public async Task ReGeneratePublishedVacanciesAsync()
        {            
            var liveVacancyIdsTask = _vacancyQuery.GetVacanciesByStatusAsync<VacancyIdentifier>(VacancyStatus.Live);
            var closedVacancyIdsTask = _vacancyQuery.GetVacanciesByStatusAsync<VacancyIdentifier>(VacancyStatus.Closed);
            var programmesTask = _apprenticeshipProgrammeProvider.GetApprenticeshipProgrammesAsync();

            await Task.WhenAll(liveVacancyIdsTask, programmesTask, closedVacancyIdsTask);

            var liveVacancyIds = liveVacancyIdsTask.Result.Select(v => v.Id).ToList();
            var closedVacancyIds = closedVacancyIdsTask.Result.Select(v => v.Id).ToList();
            var programmes = programmesTask.Result.Select(c=>(ApprenticeshipProgramme)c).ToList();
            
            var regenerateLiveVacanciesViewsTask = RegenerateLiveVacancies(liveVacancyIds, programmes);
            var regenerateClosedVacanciesViewsTask = RegenerateClosedVacancies(closedVacancyIds, programmes);

            await Task.WhenAll(regenerateClosedVacanciesViewsTask, regenerateLiveVacanciesViewsTask);
        }

        private async Task RegenerateLiveVacancies(List<Guid> liveVacancyIds, List<ApprenticeshipProgramme> programmes)
        {
            var watch = Stopwatch.StartNew();

            var deletedCount = await _queryStoreWriter.DeleteAllLiveVacancies();

            foreach (var vacancyId in liveVacancyIds)
            {
                var vacancy = await _vacancyQuery.GetVacancyAsync(vacancyId);

                var programme = programmes.Single(p => p.Id == vacancy.ProgrammeId);
                var vacancyProjection = vacancy.ToVacancyProjectionBase<LiveVacancy>(programme,
                    () => QueryViewType.LiveVacancy.GetIdValue(vacancy.VacancyReference.ToString()), _timeProvider);

                await _queryStoreWriter.UpdateLiveVacancyAsync(vacancyProjection);
            }

            watch.Stop();

            _logger.LogInformation("Recreated LiveVacancy projections. Deleted:{deletedCount}. Inserted:{insertedCount}. executionTime:{executionTimeMs}ms", deletedCount, liveVacancyIds.Count, watch.ElapsedMilliseconds);
        }

        private async Task RegenerateClosedVacancies(List<Guid> closedVacancyIds, List<ApprenticeshipProgramme> programmes)
        {
            var watch = Stopwatch.StartNew();

            var deletedCount = await _queryStoreWriter.DeleteAllClosedVacancies();

            foreach (var vacancyId in closedVacancyIds)
            {
                var vacancy = await _vacancyQuery.GetVacancyAsync(vacancyId);

                var programme = programmes.Single(p => p.Id == vacancy.ProgrammeId);
                var vacancyProjection = vacancy.ToVacancyProjectionBase<ClosedVacancy>(programme,
                    () => QueryViewType.ClosedVacancy.GetIdValue(vacancy.VacancyReference.ToString()), _timeProvider);

                await _queryStoreWriter.UpdateClosedVacancyAsync(vacancyProjection);
            }

            watch.Stop();

            _logger.LogInformation("Recreated ClosedVacancy projections. Deleted:{deletedCount}. Inserted:{insertedCount}. executionTime:{executionTimeMs}ms", deletedCount, closedVacancyIds.Count, watch.ElapsedMilliseconds);
        }
    }
}
