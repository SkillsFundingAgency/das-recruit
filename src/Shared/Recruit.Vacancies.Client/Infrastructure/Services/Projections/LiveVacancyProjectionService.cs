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
    public class LiveVacancyProjectionService : ILiveVacancyProjectionService
    {
        private readonly ILogger<LiveVacancyProjectionService> _logger;
        private readonly IQueryStoreWriter _queryStoreWriter;
        private readonly IVacancyRepository _repository;
        private readonly IReferenceDataReader _referenceDataReader;

        public LiveVacancyProjectionService(
                                    ILogger<LiveVacancyProjectionService> logger, 
                                    IQueryStoreWriter queryStoreWriter, 
                                    IVacancyRepository repository,
                                    IReferenceDataReader referenceDataReader)
        {
            _logger = logger;
            _queryStoreWriter = queryStoreWriter;
            _repository = repository;
            _referenceDataReader = referenceDataReader;
        }

        public async Task ReGenerateLiveVacanciesAsync()
        {
            var vacanciesTask = _repository.GetVacanciesByStatusAsync(VacancyStatus.Live);
            var programmesTask = _referenceDataReader.GetReferenceData<ApprenticeshipProgrammes>();

            await Task.WhenAll(vacanciesTask, programmesTask);

            var vacancies = vacanciesTask.Result.ToList();
            var programmesData = programmesTask.Result;

            _logger.LogInformation($"Found {vacancies.Count()} live vacancies to create LiveVacancy queryViews for.");

            var liveVacancies = vacancies.Select(v =>
            {
                var programme = programmesData.Data.Single(p => p.Id == v.ProgrammeId);
                return v.ToVacancyProjectionBase<LiveVacancy>(programme, () => QueryViewType.LiveVacancy.GetIdValue(v.VacancyReference.ToString()));
            });

            await _queryStoreWriter.RecreateLiveVacancies(liveVacancies);
        }
    }
}
