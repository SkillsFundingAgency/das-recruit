using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Authorization;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.GovUK.Auth.Employer;

namespace Esfa.Recruit.Employer.Web.Middleware;

public class EmployerAccountAuthorizationHandler(
    IHttpContextAccessor httpContextAccessor,
    IGovAuthEmployerAccountService accountsService,
    ILogger<EmployerAccountAuthorizationHandler> logger)
    : IEmployerAccountAuthorizationHandler
{
    public Task<bool> IsEmployerAuthorized(AuthorizationHandlerContext context, EmployerUserRole minimumAllowedRole)
    {
        try
        {
            if (!httpContextAccessor.HttpContext.Request.RouteValues.TryGetValue(RouteValueKeys.EmployerAccountHashedId, out var routeValue))
            {
                return Task.FromResult(false);
            }
            var accountIdFromUrl = routeValue.ToString().ToUpper();
            var employerAccountClaim = context.User.FindFirst(c=>c.Type.Equals(EmployerRecruitClaims.AccountsClaimsTypeIdentifier));

            if(employerAccountClaim?.Value == null)
                return Task.FromResult(false);

            Dictionary<string, EmployerUserAccountItem> employerAccounts;

            try
            {
                employerAccounts = JsonConvert.DeserializeObject<Dictionary<string, EmployerUserAccountItem>>(employerAccountClaim.Value);
            }
            catch (JsonSerializationException e)
            {
                logger.LogError(e, "Could not deserialize employer account claim for user");
                return Task.FromResult(false);
            }

            EmployerUserAccountItem employerIdentifier = null;

            // read the employer Identifier from the accounts.
            if (accountIdFromUrl != null && employerAccounts.TryGetValue(accountIdFromUrl, out EmployerUserAccountItem employerAccount))
            {
                employerIdentifier = employerAccount;
            }

            if (employerAccounts == null || !employerAccounts.ContainsKey(accountIdFromUrl))
            {
                if (!context.User.HasClaim(c => c.Type.Equals(ClaimTypes.NameIdentifier)))
                    return Task.FromResult(false);
            
                var userClaim = context.User.Claims
                    .First(c => c.Type.Equals(ClaimTypes.NameIdentifier));

                var email = context.User.Claims.FirstOrDefault(c => c.Type.Equals(ClaimTypes.Email))?.Value;

                var userId = userClaim.Value;

                var result = accountsService.GetUserAccounts(userId, email).Result;
            
                var accountsAsJson = JsonConvert.SerializeObject(result.EmployerAccounts.ToDictionary(k => k.AccountId));
                var associatedAccountsClaim = new Claim(EmployerRecruitClaims.AccountsClaimsTypeIdentifier, accountsAsJson, JsonClaimValueTypes.Json);
            
                var updatedEmployerAccounts = JsonConvert.DeserializeObject<Dictionary<string, EmployerUserAccountItem>>(associatedAccountsClaim.Value);

                userClaim.Subject?.AddClaim(associatedAccountsClaim);
            
                if (!updatedEmployerAccounts.TryGetValue(accountIdFromUrl, out EmployerUserAccountItem updatedEmployerAccount))
                {
                    return Task.FromResult(false);
                }
                employerIdentifier = updatedEmployerAccount;
            }
        
            return Task.FromResult(CheckUserRoleForAccess(employerIdentifier, minimumAllowedRole));
        }
        catch (Exception exception)
        {
            return Task.FromException<bool>(exception);
        }
    }

    private static bool CheckUserRoleForAccess(EmployerUserAccountItem employerIdentifier, EmployerUserRole minimumAllowedRole)
    {
        var tryParse = Enum.TryParse<EmployerUserRole>(employerIdentifier.Role, true, out var userRole);

        if (!tryParse)
        {
            return false;
        }

        return minimumAllowedRole switch
        {
            EmployerUserRole.Owner => userRole is EmployerUserRole.Owner,
            EmployerUserRole.Transactor => userRole is EmployerUserRole.Owner or EmployerUserRole.Transactor,
            EmployerUserRole.Viewer => userRole is EmployerUserRole.Owner or EmployerUserRole.Transactor
                or EmployerUserRole.Viewer,
            _ => false
        };
    }
}