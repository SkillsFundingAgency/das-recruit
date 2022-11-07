using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Employer.Web.Configuration;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SFA.DAS.GovUK.Auth.Services;

namespace Esfa.Recruit.Employer.Web.AppStart;

public class EmployerAccountPostAuthenticationClaimsHandler : ICustomClaims
{
    private readonly IEmployerAccountService _accountsSvc;
    private readonly IConfiguration _configuration;
    private readonly RecruitConfiguration _recruitConfiguration;

    public EmployerAccountPostAuthenticationClaimsHandler(IEmployerAccountService accountsSvc, IConfiguration configuration, IOptions<RecruitConfiguration> recruitConfiguration)
    {
        _accountsSvc = accountsSvc;
        _configuration = configuration;
        _recruitConfiguration = recruitConfiguration.Value;
    }
    public async Task<IEnumerable<Claim>> GetClaims(TokenValidatedContext ctx)
    {
        if (_configuration["StubAuth"] != null && _configuration["StubAuth"]
                .Equals("true", StringComparison.CurrentCultureIgnoreCase))
        {
            var accountClaims = new Dictionary<string, EmployerUserAccountItem>();
            accountClaims.Add("", new EmployerUserAccountItem
            {
                Role = "Owner",
                AccountId = "ABC123",
                EmployerName = "Stub Employer"
            });
            var claims = new[]
            {
                new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonConvert.SerializeObject(accountClaims)),
                new Claim(EmployerClaims.EmployerEmailClaimsTypeIdentifier, _configuration["NoAuthEmail"]),
                new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, Guid.NewGuid().ToString())
            };
            return claims.ToList();
        }

        string userId;
        var email = string.Empty;
        if (_recruitConfiguration.UseGovSignIn)
        {
            userId = ctx.Principal.Claims
                .First(c => c.Type.Equals(ClaimTypes.NameIdentifier))
                .Value;
            email = ctx.Principal.Claims
                .First(c => c.Type.Equals(ClaimTypes.Email))
                .Value;
        }
        else
        {
            userId = ctx.Principal.Claims
                .First(c => c.Type.Equals(EmployerClaims.IdamsUserIdClaimTypeIdentifier))
                .Value;
        }

        var result = await _accountsSvc.GetUserAccounts(userId, email);

        var accountsAsJson = JsonConvert.SerializeObject(result.EmployerAccounts.ToDictionary(k => k.AccountId));
        var associatedAccountsClaim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, accountsAsJson, JsonClaimValueTypes.Json);
        return new List<Claim> { associatedAccountsClaim };
    }
}