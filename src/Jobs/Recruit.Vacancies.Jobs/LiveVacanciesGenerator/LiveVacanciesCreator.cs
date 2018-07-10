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
            var programmeTask = _queryStoreReader.GetApprenticeshipProgrammesAsync();

            await Task.WhenAll(vacanciesTask, programmeTask);

            var vacancies = vacanciesTask.Result;

            _logger.LogInformation($"Found {vacancies.Count()} live vacancies to create LiveVacancy queryViews for.");

            Parallel.ForEach(vacancies, async v =>
            {
                var programme = programmeTask.Result.Programmes.Single(p => p.Id == v.ProgrammeId);
                await _queryStoreWriter.UpdateLiveVacancyAsync(v.ToLiveVacancyProjection(programme));
            });

            await Task.CompletedTask;
        }
    }
}
