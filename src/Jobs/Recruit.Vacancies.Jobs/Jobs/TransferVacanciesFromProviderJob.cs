using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Queues;
using Esfa.Recruit.Vacancies.Client.Application.Queues.Messages;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;

namespace Esfa.Recruit.Vacancies.Jobs.Jobs
{
    public class TransferVacanciesFromProviderJob
    {
        private readonly IVacancyQuery _vacanciesQuery;
        private readonly IRecruitQueueService _queueService;
        private readonly IQueryStoreReader _queryStoreReader;

        public TransferVacanciesFromProviderJob(IVacancyQuery vacanciesQuery, IRecruitQueueService queueService, IQueryStoreReader queryStoreReader)
        {
            _vacanciesQuery = vacanciesQuery;
            _queueService = queueService;
            _queryStoreReader = queryStoreReader;
        }

        public async Task Run(long ukprn, string employerAccountId, string accountLegalEntityPublicHashedId, Guid userRef, string userEmail, string userName, TransferReason transferReason)
        {
            var vacanciesTask = _vacanciesQuery.GetProviderOwnedVacanciesForLegalEntityAsync(ukprn, accountLegalEntityPublicHashedId);
            var vacanciesWithoutLegalEntityIdTask = GetProviderOwnerVacanciesWithoutLegalEntityThatMustBeTransferred(ukprn, employerAccountId, accountLegalEntityPublicHashedId);

            await Task.WhenAll(vacanciesTask, vacanciesWithoutLegalEntityIdTask);

            var vacancies = vacanciesTask.Result.Concat(vacanciesWithoutLegalEntityIdTask.Result);

            var tasks = vacancies.Select(vac => _queueService.AddMessageAsync(new TransferVacancyToLegalEntityQueueMessage
            {
                VacancyReference = vac.VacancyReference.Value,
                UserRef = userRef,
                UserEmailAddress = userEmail,
                UserName = userName,
                TransferReason = transferReason
            }));
            
            await Task.WhenAll(tasks);
        }

        private async Task<IEnumerable<Vacancy>> GetProviderOwnerVacanciesWithoutLegalEntityThatMustBeTransferred(long ukprn, string employerAccountId, string accountLegalEntityPublicHashedId)
        {
            var employer = await _queryStoreReader.GetProviderEmployerVacancyDataAsync(ukprn, employerAccountId);
            var remainingLegalEntitiesCount = employer?.LegalEntities.Count(
                                                  l => l.AccountLegalEntityPublicHashedId != accountLegalEntityPublicHashedId) ?? 0;

            //We should only transfer vacancies without a legalEntityId when the provider cannot choose another legal entity
            if (remainingLegalEntitiesCount == 0)
                return await _vacanciesQuery.GetProviderOwnedVacanciesForEmployerWithoutLegalEntityAsync(ukprn, employerAccountId);

            return Enumerable.Empty<Vacancy>();
        }
    }
}