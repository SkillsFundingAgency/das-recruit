using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using MediatR;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class CloseExpiredVacanciesCommandHandler : IRequestHandler<CloseExpiredVacanciesCommand, Unit>
    {
        private readonly ILogger<CloseExpiredVacanciesCommandHandler> _logger;
        private readonly IVacancyQuery _query;
        private readonly IQueryStoreReader _queryStoreReader;
        private readonly ITimeProvider _timeProvider;
        private readonly IVacancyService _vacancyService;
        private readonly IMessaging _messaging;

        public CloseExpiredVacanciesCommandHandler(
            ILogger<CloseExpiredVacanciesCommandHandler> logger,
            IVacancyQuery query,
            ITimeProvider timeProvider,
            IVacancyService vacancyService,
            IQueryStoreReader queryStoreReader,
            IMessaging messaging)
        {
            _logger = logger;
            _query = query;
            _timeProvider = timeProvider;
            _vacancyService = vacancyService;
            _queryStoreReader = queryStoreReader;
            _messaging = messaging;
        }

        public async Task<Unit> Handle(CloseExpiredVacanciesCommand message, CancellationToken cancellationToken)
        {
            var vacancies = (await _query.GetVacanciesByStatusAndClosingDateAsync(VacancyStatus.Live, _timeProvider.Today));
            var numberClosed = 0;

            
            var hasDocuments = await vacancies.MoveNextAsync(cancellationToken);
            
            while (hasDocuments)
            {   
                foreach (var vacancy in vacancies.Current)
                {
                    _logger.LogInformation($"Closing vacancy {vacancy.VacancyReference} with closing date of {vacancy.ClosingDate}");
                    await _vacancyService.CloseExpiredVacancy(vacancy.Id);
                    numberClosed++;
                }

                hasDocuments = await vacancies.MoveNextAsync(cancellationToken);
            }
            

            _logger.LogInformation("Closed {closedCount} live vacancies", numberClosed);

            var orphanedVacancies = (await _queryStoreReader.GetLiveExpiredVacancies(_timeProvider.Today)).ToList();

            foreach (var orphanedVacancy in orphanedVacancies)
            {
                await _messaging.PublishEvent(new VacancyClosedEvent
                {
                    VacancyId = orphanedVacancy.VacancyId,
                    VacancyReference = orphanedVacancy.VacancyReference
                });
            }
            
            _logger.LogInformation("Closed {closedCount} orphaned live vacancies", orphanedVacancies.Count);
            
            return Unit.Value;
        }
    }
}