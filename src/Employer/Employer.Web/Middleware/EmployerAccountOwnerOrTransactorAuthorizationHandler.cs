using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace Esfa.Recruit.Employer.Web.Middleware;

public class EmployerAccountOwnerOrTransactorAuthorizationHandler(IEmployerAccountAuthorizationHandler handler)
    : AuthorizationHandler<EmployerAccountOwnerOrTransactorRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, EmployerAccountOwnerOrTransactorRequirement ownerOrTransactorRequirement)
    {
        if (!await handler.IsEmployerAuthorized(context, EmployerUserRole.Transactor))
        {
            return;
        }
        context.Succeed(ownerOrTransactorRequirement);
    }
}