using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;

namespace Esfa.Recruit.Shared.Web.Services
{
    public class LegalEntityAgreementService : ILegalEntityAgreementService
    {
        private readonly IEmployerVacancyClient _client;

        public LegalEntityAgreementService(IEmployerVacancyClient client)
        {
            _client = client;
        }

        public async Task<bool> HasLegalEntityAgreementAsync(string employerAccountId, string accountLegalEntityPublicHashedId)
        {
            var legalEntity = await GetLegalEntityAsync(employerAccountId, accountLegalEntityPublicHashedId);

             return await HasLegalEntityAgreementAsync(employerAccountId, legalEntity);
        }

        public async Task<bool> HasLegalEntityAgreementAsync(string employerAccountId, LegalEntity legalEntity)
        {
            if (legalEntity == null)
                return false;

            if (legalEntity.HasLegalEntityAgreement)
                return true;

            //Agreement may have been signed since the projection was created. Check Employer Service.
            var hasLegalEntityAgreement = await CheckEmployerServiceForLegalEntityAgreementAsync(employerAccountId, legalEntity.AccountLegalEntityPublicHashedId);

            if (hasLegalEntityAgreement)
            {
                await _client.SetupEmployerAsync(employerAccountId);
            }

            return hasLegalEntityAgreement;
        }

        public async Task<LegalEntity> GetLegalEntityAsync(string employerAccountId, string accountLegalEntityPublicHashedId)
        {
            var employerData = await _client.GetEditVacancyInfoAsync(employerAccountId);

            var legalEntity = employerData.LegalEntities.SingleOrDefault(l => l.AccountLegalEntityPublicHashedId == accountLegalEntityPublicHashedId);

            return legalEntity;
        }

       private async Task<bool> CheckEmployerServiceForLegalEntityAgreementAsync(string employerAccountId, string accountLegalEntityPublicHashedId)
        {
            var legalEntities = await _client.GetEmployerLegalEntitiesAsync(employerAccountId);

            var legalEntity = legalEntities.SingleOrDefault(e => e.AccountLegalEntityPublicHashedId == accountLegalEntityPublicHashedId);

            return legalEntity?.HasLegalEntityAgreement ?? false;
        }
    }
}
