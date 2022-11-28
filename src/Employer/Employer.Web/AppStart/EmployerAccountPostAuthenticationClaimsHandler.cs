using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Newtonsoft.Json;
using SFA.DAS.GovUK.Auth.Services;

namespace Esfa.Recruit.Employer.Web.AppStart;

public class EmployerAccountPostAuthenticationClaimsHandler : ICustomClaims
{
    private readonly IRecruitVacancyClient _vacancyClient;

    public EmployerAccountPostAuthenticationClaimsHandler(IRecruitVacancyClient vacancyClient)
    {
        _vacancyClient = vacancyClient;
    }
    public async Task<IEnumerable<Claim>> GetClaims(TokenValidatedContext ctx)
    {
        var userId = ctx.Principal.Claims
            .First(c => c.Type.Equals(ClaimTypes.NameIdentifier))
            .Value;
        var email = ctx.Principal.Claims
            .First(c => c.Type.Equals(ClaimTypes.Email))
            .Value;
    

        var accounts = await _vacancyClient.GetEmployerIdentifiersAsync(userId, email);
        var accountsAsJson = JsonConvert.SerializeObject(accounts.UserAccounts.Select(c=>c.AccountId).ToList());
        var associatedAccountsClaim = new Claim(EmployerRecruitClaims.AccountsClaimsTypeIdentifier, accountsAsJson, JsonClaimValueTypes.Json);
        
        return new List<Claim> { associatedAccountsClaim, new Claim(EmployerRecruitClaims.IdamsUserIdClaimTypeIdentifier,accounts.EmployerUserId) };
    }
}