using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Extensions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData;
using RefData = Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.ApprenticeshipProgrammes;

namespace Esfa.Recruit.Vacancies.Jobs.LiveVacanciesGenerator
{
    public class LiveVacanciesCreator
    {
        private readonly ILogger<LiveVacanciesCreator> _logger;
        private readonly IQueryStoreWriter _queryStoreWriter;
        private readonly IVacancyRepository _repository;
        private readonly IReferenceDataReader _referenceDataReader;

        public LiveVacanciesCreator(ILogger<LiveVacanciesCreator> logger, 
                                    IQueryStoreWriter queryStoreWriter, 
                                    IVacancyRepository repository,
                                    IReferenceDataReader referenceDataReader)
        {
            _logger = logger;
            _queryStoreWriter = queryStoreWriter;
            _repository = repository;
            _referenceDataReader = referenceDataReader;
        }

        public async Task RunAsync()
        {
            var vacanciesTask = _repository.GetVacanciesByStatusAsync(Client.Domain.Entities.VacancyStatus.Live);
            var programmesTask = _referenceDataReader.GetReferenceData<RefData.ApprenticeshipProgrammes>();

            await Task.WhenAll(vacanciesTask, programmesTask);

            var vacancies = vacanciesTask.Result.ToList();
            var programmesData = programmesTask.Result;

            _logger.LogInformation($"Found {vacancies.Count()} live vacancies to create LiveVacancy queryViews for.");

            var liveVacancies = vacancies.Select(v =>
            {
                var programme = programmesData.Data.Single(p => p.Id == v.ProgrammeId);
                return v.ToLiveVacancyProjection(programme);
            });

            await _queryStoreWriter.RecreateLiveVacancies(liveVacancies);
        }
    }
}