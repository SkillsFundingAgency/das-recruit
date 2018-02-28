using Esfa.Recruit.Employer.Web.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Services
{
    public interface IEmployerAccountService
    {
        Task<IDictionary<string, EmployerIdentifier>> GetEmployerIdentifiersAsync(string userId);
    }
}
