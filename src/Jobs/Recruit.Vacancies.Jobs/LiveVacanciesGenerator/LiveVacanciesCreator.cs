using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Extensions;

namespace Esfa.Recruit.Vacancies.Jobs.LiveVacanciesGenerator
{
    public class LiveVacanciesCreator
    {
        private readonly ILogger<LiveVacanciesCreator> _logger;
        private readonly IQueryStoreReader _queryStoreReader;
        private readonly IQueryStoreWriter _queryStoreWriter;
        private readonly IVacancyRepository _repository;

        public LiveVacanciesCreator(ILogger<LiveVacanciesCreator> logger, IQueryStoreReader queryStoreReader, 
                                    IQueryStoreWriter queryStoreWriter, IVacancyRepository repository)
        {
            _logger = logger;
            _queryStoreReader = queryStoreReader;
            _queryStoreWriter = queryStoreWriter;
            _repository = repository;
        }

        public async Task RunAsync()
        {
            var vacanciesTask = _repository.GetVacanciesByStatusAsync(Client.Domain.Entities.VacancyStatus.Live);
            var programmesTask = _queryStoreReader.GetApprenticeshipProgrammesAsync();

            await Task.WhenAll(vacanciesTask, programmesTask);

            var vacancies = vacanciesTask.Result.ToList();
            var programmesData = programmesTask.Result;

            _logger.LogInformation($"Found {vacancies.Count()} live vacancies to create LiveVacancy queryViews for.");

            var liveVacancies = vacancies.Select(v =>
            {
                var programme = programmesData.Programmes.Single(p => p.Id == v.ProgrammeId);
                return v.ToLiveVacancyProjection(programme);
            });

            await _queryStoreWriter.RecreateLiveVacancies(liveVacancies);
        }
    }
}