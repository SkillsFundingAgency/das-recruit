using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Queues;
using Esfa.Recruit.Vacancies.Client.Application.Queues.Messages;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.Jobs
{
    public class TransferVacanciesFromProviderJob
    {
        private readonly IVacancyQuery _vacanciesQuery;
        private readonly IRecruitQueueService _queueService;
        private readonly IQueryStoreReader _queryStoreReader;
        private readonly ILogger<TransferVacanciesFromProviderJob> _logger;

        public TransferVacanciesFromProviderJob(IVacancyQuery vacanciesQuery, IRecruitQueueService queueService, IQueryStoreReader queryStoreReader, ILogger<TransferVacanciesFromProviderJob> logger)
        {
            _vacanciesQuery = vacanciesQuery;
            _queueService = queueService;
            _queryStoreReader = queryStoreReader;
            _logger = logger;
        }

        public async Task Run(long ukprn, string employerAccountId, string accountLegalEntityPublicHashedId, Guid userRef, string userEmail, string userName, TransferReason transferReason)
        {
            try
            {
                var vacanciesTask = _vacanciesQuery.GetProviderOwnedVacanciesForLegalEntityAsync(ukprn, accountLegalEntityPublicHashedId);
                var vacanciesWithoutLegalEntityIdTask = GetProviderOwnerVacanciesWithoutLegalEntityThatMustBeTransferred(ukprn, employerAccountId, accountLegalEntityPublicHashedId);

                await Task.WhenAll(vacanciesTask, vacanciesWithoutLegalEntityIdTask);

                var vacancies = vacanciesTask.Result.Concat(vacanciesWithoutLegalEntityIdTask.Result);

                var tasks = vacancies.Select(vac => 
                    _queueService.AddMessageAsync(new TransferVacancyToLegalEntityQueueMessage
                {
                    VacancyReference = vac.VacancyReference.Value,
                    UserRef = userRef,
                    UserEmailAddress = userEmail,
                    UserName = userName,
                    TransferReason = transferReason
                }));

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while transferring vacancies when handling Provider " +
                                     $"permission being revoked by Employer.");
            }
            
        }

        private async Task<IEnumerable<Vacancy>> GetProviderOwnerVacanciesWithoutLegalEntityThatMustBeTransferred(long ukprn, string employerAccountId, string accountLegalEntityPublicHashedId)
        {
            var employer = await _queryStoreReader.GetProviderEmployerVacancyDataAsync(ukprn, employerAccountId);
            var remainingLegalEntitiesCount = employer?.LegalEntities.Count(
                                                  l => l.AccountLegalEntityPublicHashedId != accountLegalEntityPublicHashedId) ?? 0;

            //We should only transfer vacancies without a accountLegalEntityPublicHashedId when the provider cannot choose another legal entity
            if (remainingLegalEntitiesCount == 0)
                return await _vacanciesQuery.GetProviderOwnedVacanciesForEmployerWithoutAccountLegalEntityPublicHashedIdAsync(ukprn, employerAccountId);

            return Enumerable.Empty<Vacancy>();
        }
    }
}