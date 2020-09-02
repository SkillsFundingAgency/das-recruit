using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;


namespace Esfa.Recruit.Shared.Web.Services
{
    public interface ILegalEntityAgreementService
    {
        Task<bool> HasLegalEntityAgreementAsync(string employerAccountId, string accountLegalEntityPublicHashedId);
        Task<LegalEntity> GetLegalEntityAsync(string employerAccountId, string accountLegalEntityPublicHashedId);
        Task<bool> HasLegalEntityAgreementAsync(string employerAccountId, LegalEntity legalEntity);
    }
}