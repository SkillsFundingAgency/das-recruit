using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace Esfa.Recruit.Employer.Web.Middleware;

public class EmployerAccountOwnerOrTransactorAuthorizationHandler : AuthorizationHandler<EmployerAccountOwnerOrTransactorRequirement>
{
    private readonly IEmployerAccountAuthorizationHandler _handler;

    public EmployerAccountOwnerOrTransactorAuthorizationHandler(IEmployerAccountAuthorizationHandler handler)
    {
        _handler = handler;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, EmployerAccountOwnerOrTransactorRequirement ownerOrTransactorRequirement)
    {
        if (!await _handler.IsEmployerAuthorized(context, EmployerUserRole.Transactor))
        {
            return;
        }
        context.Succeed(ownerOrTransactorRequirement);
    }
}