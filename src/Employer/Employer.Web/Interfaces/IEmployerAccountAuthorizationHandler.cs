using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Interfaces
{
    public interface IEmployerAccountAuthorizationHandler
    {
        /// <summary>
        /// Contract to validate if the logged in user is authorized and got right roles associated.
        /// </summary>
        /// <param name="context">AuthorizationHandlerContext.</param>
        /// <param name="allowAllUserRoles">Boolean.</param>
        /// <returns>boolean.</returns>
        Task<bool> IsEmployerAuthorized(AuthorizationHandlerContext context, bool allowAllUserRoles);
    }
}
