using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;

namespace Esfa.Recruit.Shared.Web.Services
{
    public interface ILegalEntityAgreementService
    {
        Task<bool> HasLegalEntityAgreementAsync(string employerAccountId, long legalEntityId);
        Task<LegalEntity> GetLegalEntityAsync(string employerAccountId, long legalEntityId);
    }
}