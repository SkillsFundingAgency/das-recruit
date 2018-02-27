using Esfa.Recruit.Employer.Web.Models;
using SFA.DAS.EAS.Account.Api.Types;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Services
{
    public interface IEmployerAccountService
    {
        Task<Dictionary<string, EmployerIdentifier>> GetEmployerIdentifiersAsync(string userId);
    }
}
