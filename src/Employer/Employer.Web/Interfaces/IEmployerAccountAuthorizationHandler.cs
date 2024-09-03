using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Middleware;

namespace Esfa.Recruit.Employer.Web.Interfaces
{
    public interface IEmployerAccountAuthorizationHandler
    {
        Task<bool> IsEmployerAuthorized(AuthorizationHandlerContext context, EmployerUserRole minimumAllowedRole);
    }
}
