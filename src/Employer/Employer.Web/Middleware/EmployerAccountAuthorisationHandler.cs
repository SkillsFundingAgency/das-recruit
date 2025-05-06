using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Authorization;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Interfaces;
using Esfa.Recruit.Employer.Web.Models;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.EmployerAccount;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.GovUK.Auth.Employer;

namespace Esfa.Recruit.Employer.Web.Middleware;

public class EmployerAccountAuthorizationHandler : IEmployerAccountAuthorizationHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IGovAuthEmployerAccountService _accountsService;
    private readonly ILogger<EmployerAccountAuthorizationHandler> _logger;

    public EmployerAccountAuthorizationHandler(IHttpContextAccessor httpContextAccessor, IGovAuthEmployerAccountService accountsService, ILogger<EmployerAccountAuthorizationHandler> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _accountsService = accountsService;
        _logger = logger;
    }
    
    public async Task<bool> IsEmployerAuthorized(AuthorizationHandlerContext context, EmployerUserRole minimumAllowedRole)
    {
        if (!_httpContextAccessor.HttpContext.Request.RouteValues.ContainsKey(RouteValueKeys.EmployerAccountHashedId))
        {
            return false;
        }
        var accountIdFromUrl = _httpContextAccessor.HttpContext.Request.RouteValues[RouteValueKeys.EmployerAccountHashedId].ToString().ToUpper();
        var employerAccountClaim = context.User.FindFirst(c=>c.Type.Equals(EmployerRecruitClaims.AccountsClaimsTypeIdentifier));

        if(employerAccountClaim?.Value == null)
            return false;

        Dictionary<string, EmployerUserAccountItem> employerAccounts;

        try
        {
            employerAccounts = JsonConvert.DeserializeObject<Dictionary<string, EmployerUserAccountItem>>(employerAccountClaim.Value);
        }
        catch (JsonSerializationException e)
        {
            _logger.LogError(e, "Could not deserialize employer account claim for user");
            return false;
        }

        EmployerUserAccountItem employerIdentifier = null;

        // read the employer Identifier from the accounts.
        if (accountIdFromUrl != null && employerAccounts.ContainsKey(accountIdFromUrl))
        {
            employerIdentifier = employerAccounts[accountIdFromUrl];
        }

        if (employerAccounts == null || !employerAccounts.ContainsKey(accountIdFromUrl))
        {
            if (!context.User.HasClaim(c => c.Type.Equals(ClaimTypes.NameIdentifier)))
                return false;
            
            var userClaim = context.User.Claims
                .First(c => c.Type.Equals(ClaimTypes.NameIdentifier));

            var email = context.User.Claims.FirstOrDefault(c => c.Type.Equals(ClaimTypes.Email))?.Value;

            var userId = userClaim.Value;

            var result = _accountsService.GetUserAccounts(userId, email).Result;
            
            var accountsAsJson = JsonConvert.SerializeObject(result.EmployerAccounts.ToDictionary(k => k.AccountId));
            var associatedAccountsClaim = new Claim(EmployerRecruitClaims.AccountsClaimsTypeIdentifier, accountsAsJson, JsonClaimValueTypes.Json);
            
            var updatedEmployerAccounts = JsonConvert.DeserializeObject<Dictionary<string, EmployerUserAccountItem>>(associatedAccountsClaim.Value);

            userClaim.Subject.AddClaim(associatedAccountsClaim);
            
            if (!updatedEmployerAccounts.ContainsKey(accountIdFromUrl))
            {
                return false;
            }
            employerIdentifier = updatedEmployerAccounts[accountIdFromUrl];
        }
        
        return CheckUserRoleForAccess(employerIdentifier, minimumAllowedRole);
    }

    private static bool CheckUserRoleForAccess(EmployerUserAccountItem employerIdentifier, EmployerUserRole minimumAllowedRole)
    {
        bool tryParse = Enum.TryParse<EmployerUserRole>(employerIdentifier.Role, true, out var userRole);

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