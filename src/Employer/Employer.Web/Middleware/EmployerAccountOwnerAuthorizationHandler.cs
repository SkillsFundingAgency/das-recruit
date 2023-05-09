using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace Esfa.Recruit.Employer.Web.Middleware;

public class EmployerAccountOwnerAuthorizationHandler : AuthorizationHandler<EmployerAccountOwnerRequirement>
{
    private readonly IEmployerAccountAuthorizationHandler _handler;

    public EmployerAccountOwnerAuthorizationHandler(IEmployerAccountAuthorizationHandler handler)
    {
        _handler = handler;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, EmployerAccountOwnerRequirement ownerRequirement)
    {
        if (!await _handler.IsEmployerAuthorized(context, false))
        {
            return;
        }
        context.Succeed(ownerRequirement);
    }
}