using System;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Queues;
using Esfa.Recruit.Vacancies.Client.Application.Queues.Messages;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;

namespace Esfa.Recruit.Vacancies.Jobs
{
    public class TransferVacanciesFromProviderJob
    {
        private readonly IVacancyQuery _vacanciesQuery;
        private readonly IRecruitQueueService _queueService;

        public TransferVacanciesFromProviderJob(IVacancyQuery vacanciesQuery, IRecruitQueueService queueService)
        {
            _vacanciesQuery = vacanciesQuery;
            _queueService = queueService;
        }

        public async Task Run(long ukprn, long legalEntityId, Guid userRef, string userEmail, string userName, TransferReason transferReason)
        {
            var vacancies = await _vacanciesQuery.GetProviderOwnedVacanciesForLegalEntityAsync(ukprn, legalEntityId);

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
    }
}