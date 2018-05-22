using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Esfa.Recruit.Employer.Web.Middleware
{
    public class EmployerAccountHandler : AuthorizationHandler<EmployerAccountRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, EmployerAccountRequirement requirement)
        {
            if (context.Resource is AuthorizationFilterContext mvcContext && mvcContext.RouteData.Values.ContainsKey(RouteValues.EmployerAccountId))
            {
                if (context.User.HasClaim(c => c.Type.Equals(EmployerRecruitClaims.AccountsClaimsTypeIdentifier)))
                {
                    var accountIdFromUrl = mvcContext.RouteData.Values[RouteValues.EmployerAccountId].ToString().ToUpper();
                    var employerAccounts = context.User.GetEmployerAccounts();

                    if (employerAccounts.Contains(accountIdFromUrl))
                    {
                        mvcContext.HttpContext.Items.Add(ContextItemKeys.EmployerIdentifier, accountIdFromUrl);
                        context.Succeed(requirement);
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}