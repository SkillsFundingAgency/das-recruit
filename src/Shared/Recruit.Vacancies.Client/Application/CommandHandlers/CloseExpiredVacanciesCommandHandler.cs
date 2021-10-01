using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class CloseExpiredVacanciesCommandHandler : IRequestHandler<CloseExpiredVacanciesCommand, Unit>
    {
        private readonly ILogger<CloseExpiredVacanciesCommandHandler> _logger;
        private readonly IVacancyQuery _query;
        private readonly ITimeProvider _timeProvider;
        private readonly IVacancyService _vacancyService;

        public CloseExpiredVacanciesCommandHandler(
            ILogger<CloseExpiredVacanciesCommandHandler> logger,
            IVacancyQuery query,
            ITimeProvider timeProvider,
            IVacancyService vacancyService)
        {
            _logger = logger;
            _query = query;
            _timeProvider = timeProvider;
            _vacancyService = vacancyService;
        }

        public async Task<Unit> Handle(CloseExpiredVacanciesCommand message, CancellationToken cancellationToken)
        {
            var vacancies = (await _query.GetVacanciesByStatusAsync<VacancyIdentifier>(VacancyStatus.Live)).ToList();
            var numberClosed = 0;

            foreach (var vacancy in vacancies.Where(x => x.ClosingDate < _timeProvider.Today))
            {
                _logger.LogInformation($"Closing vacancy {vacancy.VacancyReference} with closing date of {vacancy.ClosingDate}");
                await _vacancyService.CloseExpiredVacancy(vacancy.Id);
                numberClosed++;
            }

            _logger.LogInformation("Closed {closedCount} from {liveVacancyCount} live vacancies", numberClosed, vacancies.Count);
            
            return Unit.Value;
        }
    }
}