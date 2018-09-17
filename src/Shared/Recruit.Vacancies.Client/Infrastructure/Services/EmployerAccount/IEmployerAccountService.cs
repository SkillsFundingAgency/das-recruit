using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.EAS.Account.Api.Types;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.EmployerAccount
{
    public interface IEmployerAccountService
    {
        Task<IEnumerable<string>> GetEmployerIdentifiersAsync(string userId);
        Task<IEnumerable<LegalEntityViewModel>> GetEmployerLegalEntitiesAsync(string accountId);
    }
}
