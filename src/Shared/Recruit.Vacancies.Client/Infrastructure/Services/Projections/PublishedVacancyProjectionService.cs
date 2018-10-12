using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly IVacancyRepository _repository;
        private readonly IReferenceDataReader _referenceDataReader;

        public PublishedVacancyProjectionService(
                                    ILogger<PublishedVacancyProjectionService> logger, 
                                    IQueryStoreWriter queryStoreWriter, 
                                    IVacancyRepository repository,
                                    IReferenceDataReader referenceDataReader)
        {
            _logger = logger;
            _queryStoreWriter = queryStoreWriter;
            _repository = repository;
            _referenceDataReader = referenceDataReader;
        }

        public async Task ReGeneratePublishedVacanciesAsync()
        {            
            var liveVacanciesTask = _repository.GetVacanciesByStatusAsync(VacancyStatus.Live);
            var closedVacanciesTask = _repository.GetVacanciesByStatusAsync(VacancyStatus.Closed);
            var programmesTask = _referenceDataReader.GetReferenceData<ApprenticeshipProgrammes>();

            await Task.WhenAll(liveVacanciesTask, programmesTask, closedVacanciesTask);

            var liveVacancies = liveVacanciesTask.Result.ToList();
            var programmesData = programmesTask.Result.Data;
            var closedVacancies = closedVacanciesTask.Result.ToList();

            var regenerateLiveVacanciesViewsTask = RegenerateLiveVacancies(liveVacancies, programmesData);
            var regenerateClosedVacanciesViewsTask = RegenerateClosedVacancies(closedVacancies, programmesData);

            await Task.WhenAll(regenerateClosedVacanciesViewsTask, regenerateLiveVacanciesViewsTask);
        }

        private Task RegenerateLiveVacancies(List<Vacancy> liveVacancies, List<ApprenticeshipProgramme> programmes)
        {
            _logger.LogInformation($"Found {liveVacancies.Count} live vacancies to create LiveVacancy queryViews for.");

            var projectedVacancies = liveVacancies.Select(v =>
            {
                var programme = programmes.Single(p => p.Id == v.ProgrammeId);
                return v.ToVacancyProjectionBase<LiveVacancy>(programme, () => QueryViewType.LiveVacancy.GetIdValue(v.VacancyReference.ToString()));
            });

            return _queryStoreWriter.RecreateLiveVacancies(projectedVacancies);
        }

        private Task RegenerateClosedVacancies(List<Vacancy> closedVacancies, List<ApprenticeshipProgramme> programmes)
        {
            _logger.LogInformation($"Found {closedVacancies.Count} closed vacancies to create ClosedVacancy queryViews for.");

            var projectedVacancies = closedVacancies.Select(v =>
            {
                var programme = programmes.Single(p => p.Id == v.ProgrammeId);
                return v.ToVacancyProjectionBase<ClosedVacancy>(programme, () => QueryViewType.ClosedVacancy.GetIdValue(v.VacancyReference.ToString()));
            });

            return _queryStoreWriter.RecreateClosedVacancies(projectedVacancies);
        }
    }
}
