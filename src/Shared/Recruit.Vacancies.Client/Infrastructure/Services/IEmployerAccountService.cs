using SFA.DAS.EAS.Account.Api.Types;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services
{
    public interface IEmployerAccountService
    {
        Task<IEnumerable<string>> GetEmployerIdentifiersAsync(string userId);
        Task<IEnumerable<LegalEntityViewModel>> GetEmployerLegalEntitiesAsync(string accountId);
    }
}
