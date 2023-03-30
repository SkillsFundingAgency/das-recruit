using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Authorization;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Interfaces;
using Esfa.Recruit.Employer.Web.Models;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.EmployerAccount;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Esfa.Recruit.Employer.Web.Middleware;

public class EmployerAccountAuthorizationHandler : IEmployerAccountAuthorizationHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IEmployerAccountProvider _accountsService;
    private readonly IConfiguration _configuration;

    public EmployerAccountAuthorizationHandler(IHttpContextAccessor httpContextAccessor, IEmployerAccountProvider accountsService, IConfiguration configuration)
    {
        _httpContextAccessor = httpContextAccessor;
        _accountsService = accountsService;
        _configuration = configuration;
    }
    public async Task<bool> IsEmployerAuthorized(AuthorizationHandlerContext context, bool allowAllUserRoles)
    {
        if (_httpContextAccessor.HttpContext != null
            && !_httpContextAccessor.HttpContext.Request.RouteValues.ContainsKey(RouteValueKeys.EmployerAccountHashedId)
            && _httpContextAccessor.HttpContext.User.Identity is { IsAuthenticated: false })
            return false;

        // read the accountId from the route or query string.
        string accountIdFromUrl = _httpContextAccessor.HttpContext?.Request.RouteValues[RouteValueKeys.EmployerAccountHashedId]?.ToString()?.ToUpper();
        var employerAccountClaim = context.User.FindFirst(c => c.Type.Equals(EmployerRecruitClaims.AccountsClaimsTypeIdentifier));

        // check if the employer account claim is null
        if (employerAccountClaim?.Value == null)
            return false;

        EmployerUserAccountItem employerIdentifier = null;

        string requiredIdClaim = EmployerRecruitClaims.IdamsUserIdClaimTypeIdentifier;

        if (_configuration["RecruitConfiguration:UseGovSignIn"] != null
            && _configuration["RecruitConfiguration:UseGovSignIn"].Equals("true", StringComparison.CurrentCultureIgnoreCase))
        {
            requiredIdClaim = ClaimTypes.NameIdentifier;
        }

        // check if the user claim matches with the claim identifier.
        if (!context.User.HasClaim(c => c.Type.Equals(requiredIdClaim)))
            return false;

        var userClaim = context.User.Claims.First(c => c.Type.Equals(requiredIdClaim));

        string email = context.User.Claims.FirstOrDefault(c => c.Type.Equals(EmployerRecruitClaims.IdamsUserEmailClaimTypeIdentifier))?.Value;

        var result = await _accountsService.GetEmployerIdentifiersAsync(userClaim.Value, email);

        string accountsAsJson = JsonConvert.SerializeObject(result.UserAccounts.ToDictionary(k => k.AccountId));
        var associatedAccountsClaim = new Claim(EmployerRecruitClaims.AccountsClaimsTypeIdentifier, accountsAsJson, JsonClaimValueTypes.Json);
        var employerAccounts = JsonConvert.DeserializeObject<Dictionary<string, EmployerUserAccountItem>>(associatedAccountsClaim.Value);

        // add the claims to the httpContext User.
        userClaim.Subject?.AddClaim(associatedAccountsClaim);

        // read the employer Identifier from the accounts.
        if (accountIdFromUrl != null) employerIdentifier = employerAccounts[accountIdFromUrl];

        return CheckUserRoleForAccess(employerIdentifier, allowAllUserRoles);
    }

    private static bool CheckUserRoleForAccess(EmployerUserAccountItem employerIdentifier, bool allowAllUserRoles)
    {
        return Enum.TryParse<EmployerUserRole>(employerIdentifier.Role, true, out var userRole) &&
               (allowAllUserRoles || userRole == EmployerUserRole.Owner);
    }
}