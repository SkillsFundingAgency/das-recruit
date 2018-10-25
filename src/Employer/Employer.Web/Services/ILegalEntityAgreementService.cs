using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Services
{
    public interface ILegalEntityAgreementService
    {
        Task<bool> HasLegalEntityAgreementAsync(string employerAccountId, long legalEntityId);
    }
}