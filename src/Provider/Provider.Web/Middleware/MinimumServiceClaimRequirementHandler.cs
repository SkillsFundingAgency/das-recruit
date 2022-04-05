using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Extensions;
using Microsoft.AspNetCore.Authorization;

namespace Esfa.Recruit.Provider.Web.Middleware
{
    public class MinimumServiceClaimRequirementHandler : AuthorizationHandler<MinimumServiceClaimRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, MinimumServiceClaimRequirement requirement)
        {
            if(context.User.HasPermission(requirement.MinimumServiceClaim)) context.Succeed(requirement);
            else context.Fail();

            return Task.CompletedTask;
        }
    }
}