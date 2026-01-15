using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;

namespace Esfa.Recruit.Vacancies.Jobs.Jobs
{
    public class TransferVacancyToLegalEntityJob
    {
        private readonly IVacancyRepository _vacancyRepository;
        private readonly IMessaging _messaging;

        public TransferVacancyToLegalEntityJob(IVacancyRepository vacancyRepository, IMessaging messaging)
        {
            _vacancyRepository = vacancyRepository;
            _messaging = messaging;
        }

        public async Task Run(Guid vacancyId, Guid userRef, string userEmailAddress, string userName, TransferReason transferReason)
        {
            var vacancyToTransfer = await _vacancyRepository.GetVacancyAsync(vacancyId);

            if (vacancyToTransfer != null)
            {
                await _messaging.SendCommandAsync(new TransferVacancyToLegalEntityCommand(vacancyToTransfer.Id, userRef, userEmailAddress, userName, transferReason));
            }
        }
    }
}