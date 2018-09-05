using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Esfa.Recruit.Qa.Web.Security
{
    public class UserAuthorizationService : IUserAuthorizationService
    {
        private readonly IAuthorizationService _authorizationService;

        public UserAuthorizationService(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }

        public async Task<bool> IsTeamLeadAsync(ClaimsPrincipal user)
        {
            var authResult =
                await _authorizationService.AuthorizeAsync(user, AuthorizationPolicyNames.IsUserATeamLeadPolicyName);

            return authResult.Succeeded;
        }
    }
}