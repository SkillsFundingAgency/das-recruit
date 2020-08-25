using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Communication.Types;
using Esfa.Recruit.Vacancies.Client.Application.Communications;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;
using Humanizer;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Provider
{
    public class ProviderBlockedDomainEventHandler : DomainEventHandler, IDomainEventHandler<ProviderBlockedEvent>
    {
        private readonly IQueryStoreReader _queryStoreReader;
        private readonly IVacancyQuery _vacancyQuery;
        private readonly IMessaging _messaging;
        private readonly ICommunicationQueueService _communicationQueueService;
        ILogger<ProviderBlockedDomainEventHandler> _logger;

        public ProviderBlockedDomainEventHandler(
            IQueryStoreReader queryStoreReader,
            IVacancyQuery vacancyQuery,
            IMessaging messaging,
            ICommunicationQueueService communicationQueueService,
            ILogger<ProviderBlockedDomainEventHandler> logger) : base(logger)
        {
            _queryStoreReader = queryStoreReader;
            _vacancyQuery = vacancyQuery;
            _messaging = messaging;
            _communicationQueueService = communicationQueueService;
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

            tasks.Add(RequestProviderCommunicationAsync(eventData.Ukprn));

            tasks.AddRange(RequestEmployerCommunications(vacancies, providerInfo.Employers, eventData.Ukprn));

            await Task.WhenAll(tasks);

            _logger.LogInformation($"Finished queuing required updates after provider {eventData.Ukprn} was blocked");
        }

        private List<Task> RaiseEventsToRevokePermission(IEnumerable<EmployerInfo> employers, long ukprn)
        {
            var tasks = new List<Task>();
            foreach (var employer in employers)
            {
                if(employer.LegalEntities == null) continue;

                foreach (var legalEntity in employer.LegalEntities)
                {
                    _logger.LogInformation($"Queuing to revoke provider {ukprn} permission on account {employer.EmployerAccountId} " +
                                           $"for AccountLegalEntityPublicHashedId {legalEntity.AccountLegalEntityPublicHashedId}.");
                    var providerBlockedOnLegalEntityEvent = new ProviderBlockedOnLegalEntityEvent()
                    {
                        Ukprn = ukprn,
                        EmployerAccountId = employer.EmployerAccountId,
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

        private Task RequestProviderCommunicationAsync(long ukprn)
        {
            var communicationRequest = new CommunicationRequest(
                CommunicationConstants.RequestType.ProviderBlockedProviderNotification, 
                CommunicationConstants.ParticipantResolverNames.ProviderParticipantsResolverName, 
                CommunicationConstants.ServiceName);
            communicationRequest.AddEntity(CommunicationConstants.EntityTypes.Provider, ukprn);
            communicationRequest.AddEntity(CommunicationConstants.EntityTypes.ApprenticeshipServiceConfig, null);

            return _communicationQueueService.AddMessageAsync(communicationRequest);
        }

        private List<Task> RequestEmployerCommunications(IEnumerable<ProviderVacancySummary> vacancies, IEnumerable<EmployerInfo> employers, long ukprn)
        {
            var tasks = new List<Task>();

            tasks.AddRange(GenerateCommunicationRequestsForTransferredVacancies(vacancies, ukprn));
            tasks.AddRange(GenerateCommunicationRequestsForEmployersLiveVacancies(vacancies, ukprn));
            tasks.AddRange(GenerateCommunicationRequestsForEmployersWithPermissionsOnly(vacancies, employers, ukprn));

            return tasks;
        }

        private List<Task> GenerateCommunicationRequestsForTransferredVacancies(IEnumerable<ProviderVacancySummary> vacancies, long ukprn)
        {
            var tasks = new List<Task>();

            var employerAccountsWithTransfers = vacancies.Where(v => v.VacancyOwner == OwnerType.Provider).GroupBy(v => v.EmployerAccountId);

            foreach(var employerAccountGroup in employerAccountsWithTransfers)
            {
                var noOfVacancies = employerAccountGroup.Count();
                var communicationRequest = CommunicationRequestFactory.GetProviderBlockedEmployerNotificationForTransferredVacanciesRequest(ukprn, employerAccountGroup.Key, noOfVacancies);
                tasks.Add(_communicationQueueService.AddMessageAsync(communicationRequest));
            }
            return tasks;
        }

        private List<Task> GenerateCommunicationRequestsForEmployersLiveVacancies(IEnumerable<ProviderVacancySummary> vacancies, long ukprn)
        {
            var tasks = new List<Task>();

            var employerAccountWithLiveVacancies = vacancies.Where(v => v.VacancyOwner == OwnerType.Employer && v.Status == VacancyStatus.Live).Select(v => v.EmployerAccountId).Distinct();

            foreach(var employerAccountId in employerAccountWithLiveVacancies)
            {
                var communicationRequest = CommunicationRequestFactory.GetProviderBlockedEmployerNotificationForLiveVacanciesRequest(ukprn, employerAccountId);
                
                tasks.Add(_communicationQueueService.AddMessageAsync(communicationRequest));
            }

            return tasks;
        }

        private List<Task> GenerateCommunicationRequestsForEmployersWithPermissionsOnly(IEnumerable<ProviderVacancySummary> vacancies, IEnumerable<EmployerInfo> employers, long ukprn)
        {
            var tasks = new List<Task>();
            var distinctListOfEmployersWithVacancy = vacancies.Select(v => v.EmployerAccountId).Distinct();
            var employerIdsWithPermissionOnly = employers.Where(e => distinctListOfEmployersWithVacancy.Contains(e.EmployerAccountId) == false).Select(e => e.EmployerAccountId);

            foreach(var employerAccountId in employerIdsWithPermissionOnly)
            {
                var communicationRequest = CommunicationRequestFactory.GetProviderBlockedEmployerNotificationForPermissionOnlyRequest(ukprn, employerAccountId);

                tasks.Add(_communicationQueueService.AddMessageAsync(communicationRequest));
            }

            return tasks;
        }
    }
}