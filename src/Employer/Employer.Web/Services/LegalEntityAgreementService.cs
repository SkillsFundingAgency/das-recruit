﻿using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;

namespace Esfa.Recruit.Employer.Web.Services
{
    public class LegalEntityAgreementService : ILegalEntityAgreementService
    {
        private readonly IEmployerVacancyClient _client;
        private readonly IMessaging _messaging;

        public LegalEntityAgreementService(IEmployerVacancyClient client, IMessaging messaging)
        {
            _client = client;
            _messaging = messaging;
        }

        public async Task<bool> HasLegalEntityAgreementAsync(string employerAccountId, long legalEntityId)
        {
            var legalEntity = await GetLegalEntityAsync(employerAccountId, legalEntityId);

            if(legalEntity == null)
                return false;
            
            if (legalEntity.HasLegalEntityAgreement)
                return true;

            //Agreement may have been signed since the projection was created. Check Employer Service.
            var hasLegalEntityAgreement = await CheckEmployerServiceForLegalEntityAgreementAsync(employerAccountId, legalEntity.LegalEntityId);

            if (hasLegalEntityAgreement)
            {
                await _messaging.SendCommandAsync(new SetupEmployerCommand
                {
                    EmployerAccountId = employerAccountId
                });
            }

            return hasLegalEntityAgreement;
        }

        private async Task<LegalEntity> GetLegalEntityAsync(string employerAccountId, long legalEntityId)
        {
            var employerData = await _client.GetEditVacancyInfoAsync(employerAccountId);

            var legalEntity = employerData.LegalEntities.SingleOrDefault(l => l.LegalEntityId == legalEntityId);

            return legalEntity;
        }

        private async Task<bool> CheckEmployerServiceForLegalEntityAgreementAsync(string employerAccountId, long legalEntityId)
        {
            var legalEntities = await _client.GetEmployerLegalEntitiesAsync(employerAccountId);

            var legalEntity = legalEntities.SingleOrDefault(e => e.LegalEntityId == legalEntityId);

            return legalEntity?.HasLegalEntityAgreement ?? false;
        }
    }
}
