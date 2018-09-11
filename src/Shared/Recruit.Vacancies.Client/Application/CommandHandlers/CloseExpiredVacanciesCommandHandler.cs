using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class CloseExpiredVacanciesCommandHandler : IRequestHandler<CloseExpiredVacanciesCommand>
    {
        private readonly ILogger<CloseExpiredVacanciesCommandHandler> _logger;
        private readonly IVacancyRepository _repository;
        private readonly ITimeProvider _timeProvider;
        private readonly IVacancyService _vacancyService;

        public CloseExpiredVacanciesCommandHandler(
            ILogger<CloseExpiredVacanciesCommandHandler> logger, 
            IVacancyRepository repository, 
            ITimeProvider timeProvider,
            IVacancyService vacancyService)
        {
            _logger = logger;
            _repository = repository;
            _timeProvider = timeProvider;
            _vacancyService = vacancyService;
        }

        public async Task Handle(CloseExpiredVacanciesCommand message, CancellationToken cancellationToken)
        {
            var vacancies = (await _repository.GetVacanciesByStatusAsync(VacancyStatus.Live)).ToList();
            var numberClosed = 0;
            
            foreach (var vacancy in vacancies.Where(x => x.ClosingDate <= _timeProvider.Now))
            {
                _logger.LogInformation($"Closing vacancy {vacancy.VacancyReference} with closing date of {vacancy.ClosingDate}");
                await _vacancyService.CloseVacancy(vacancy.Id, message.CommandId);
                numberClosed++;
            }
            
            _logger.LogInformation("Closed {closedCount} from {liveVacancyCount} live vacancies", numberClosed, vacancies.Count);

            await Task.CompletedTask;
        }
    }
}