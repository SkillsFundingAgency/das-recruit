using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Provider
{
    public class ProviderBlockedDomainEventHandler : DomainEventHandler, IDomainEventHandler<ProviderBlockedEvent>
    {
        private readonly IQueryStoreReader _queryStoreReader;
        private readonly IVacancyQuery _vacancyQuery;
        private readonly IMessaging _messaging;
        ILogger<ProviderBlockedDomainEventHandler> _logger;

        public ProviderBlockedDomainEventHandler(
            IQueryStoreReader queryStoreReader,
            IVacancyQuery vacancyQuery,
            IMessaging messaging,
            ILogger<ProviderBlockedDomainEventHandler> logger) : base(logger)
        {
            _queryStoreReader = queryStoreReader;
            _vacancyQuery = vacancyQuery;
            _messaging = messaging;
            _logger = logger;
        }

        public async Task HandleAsync(string eventPayload)
        {
            var eventData = DeserializeEvent<ProviderBlockedEvent>(eventPayload);

            _logger.LogInformation($"Begining to queue required updates after provider {eventData.Ukprn} was blocked");

            var tasks = new List<Task>();

            var providerInfoTask = _queryStoreReader.GetProviderVacancyDataAsync(eventData.Ukprn);
            var vacanciesTask = _vacancyQuery.GetVacanciesAssociatedToProvider(eventData.Ukprn);
            await Task.WhenAll(providerInfoTask, vacanciesTask);
            var providerInfo = providerInfoTask.Result;
            var vacancies = vacanciesTask.Result;

            if (providerInfo?.Employers != null)
            {
                tasks.AddRange(RaiseEventsToRevokePermission(providerInfo.Employers, eventData.Ukprn));
            }

            tasks.AddRange(RaiseEventsToUpdateVacancies(vacancies, eventData.QaVacancyUser, eventData.Ukprn, eventData.BlockedDate));

            // tasks.Add(RequestProviderCommunication(eventData.ProviderName));

            // tasks.AddRange(RequestEmployerCommunications(vacancies));

            await Task.WhenAll(tasks);

            _logger.LogInformation($"Finished queuing required updates after provider {eventData.Ukprn} was blocked");
        }

        private List<Task> RaiseEventsToRevokePermission(IEnumerable<EmployerInfo> employers, long ukprn)
        {
            var tasks = new List<Task>();
            foreach (var employer in employers)
            {
                foreach (var legalEntity in employer.LegalEntities)
                {
                    _logger.LogInformation($"Queuing to revoke provider {ukprn} permission on account {employer.EmployerAccountId} for legal entity {legalEntity.LegalEntityId}.");
                    var providerBlockedOnLegalEntityEvent = new ProviderBlockedOnLegalEntityEvent()
                    {
                        Ukprn = ukprn,
                        EmployerAccountId = employer.EmployerAccountId,
                        LegalEntityId = legalEntity.LegalEntityId,
                        AccountLegalEntityPublicHashedId = legalEntity.AccountLegalEntityPublicHashedId
                    };

                    tasks.Add(_messaging.PublishEvent(providerBlockedOnLegalEntityEvent));
                }
            }
            return tasks;
        }

        private List<Task> RaiseEventsToUpdateVacancies(IEnumerable<ProviderVacancySummary> vacancies, VacancyUser qaVacancyUser, long ukprn, DateTime blockedDate)
        {
            var tasks = new List<Task>();

            foreach (var vacancy in vacancies)
            {
                _logger.LogInformation($"Queuing updating of vacancy {vacancy.VacancyReference} owned by {vacancy.VacancyOwner} as the provider {ukprn} is blocked.");
                var providerBlockedOnVacancyEvent = new ProviderBlockedOnVacancyEvent()
                {
                    Ukprn = ukprn,
                    VacancyId = vacancy.Id,
                    QaVacancyUser = qaVacancyUser,
                    VacancyReference = vacancy.VacancyReference,
                    BlockedDate = blockedDate
                };

                tasks.Add(_messaging.PublishEvent(providerBlockedOnVacancyEvent));
            }

            return tasks;
        }

        private Task RequestProviderCommunication(string providerName)
        {
            //TODO 
            // - raise communication request 
            // - add DataItems collection to CommunicationRequest object
            // - add provider's name in the data items collection (no Entities are required)

            //TODO Communications
            // - modify Comm Processor to add data items from the comm request 
            //      and make sure it works having no data entites 
            // - Modify user preferences to always return email channel with immediate effect for this comm request
            // - create templates in gov notify
            // - add configs to employer config
            // - update template id provider
            return Task.CompletedTask;
        }

        private List<Task> RequestEmployerCommunications(IEnumerable<ProviderVacancySummary> vacancies)
        {
            var tasks = new List<Task>();

            //TODO 
            // - find all the vacancies OWNED by the provider
            // - get unique list of employers from the vacancy list
            // - Raise one communication request for each employer
            //      Add entity data item for BlockedProvider entity, the value will consist of the ukprn, employer account id and blocked date

            //TODO Communications
            // - Create new (or extend) user provider plugin that works directly off employer account id
            // - Create new data item plugin for the request type that will return the count of vacancies that were transferred
            //      look for vacancies associated to the employer and provider with TransferInfo marked with blocked timestamp

            return tasks;
        }
    }
}