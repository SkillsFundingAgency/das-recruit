using System;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;

namespace Esfa.Recruit.Vacancies.Jobs.Jobs
{
    public class TransferVacanciesFromEmployerReviewToQAReviewJob
    {
        private readonly IVacancyQuery _vacanciesQuery;
        private readonly IMessaging _messaging;

        public TransferVacanciesFromEmployerReviewToQAReviewJob(IVacancyQuery vacanciesQuery, IMessaging messaging)
        {
            _vacanciesQuery = vacanciesQuery;
            _messaging = messaging;
        }

        public async Task Run(long ukprn, string accountLegalEntityPublicHashedId, Guid userRef, string userEmail, string userName)
        {
            var vacancies = await _vacanciesQuery.GetProviderOwnedVacanciesInReviewAsync(ukprn, accountLegalEntityPublicHashedId);

            var tasks = vacancies.Select(vac => _messaging.SendCommandAsync(new TransferEmployerReviewToQAReviewCommand(vac.Id, userRef, userEmail, userName)));
            await Task.WhenAll(tasks);
        }
    }
}