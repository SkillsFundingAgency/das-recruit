using System.Security.Claims;
using System.Threading.Tasks;

namespace Esfa.Recruit.Qa.Web.Security
{
    public interface IUserAuthorizationService
    {
        Task<bool> IsTeamLeadAsync(ClaimsPrincipal user);
    }
}