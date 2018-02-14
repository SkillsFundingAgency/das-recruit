using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Employer.Web
{
    public partial class Startup
    {
        public void ConfigureAuthentication(IServiceCollection services)
        {
            if (_isAuthEnabled)
            {
                JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

                services.AddAuthentication(options =>
                {
                    options.DefaultScheme = "Cookies";
                    options.DefaultChallengeScheme = "oidc";
                })
                .AddCookie("Cookies", options =>
                {
                    options.AccessDeniedPath = "/Error/403";
                })
                .AddOpenIdConnect("oidc", options =>
                {
                    options.SignInScheme = "Cookies";

                    options.Authority = _authConfig.Authority;
                    options.MetadataAddress = _authConfig.MetaDataAddress;
                    options.RequireHttpsMetadata = false;
                    options.ResponseType = "code";
                    options.ClientId = _authConfig.ClientId;
                    options.ClientSecret = _authConfig.ClientSecret;
                    options.Scope.Add("profile");

                    options.Events.OnTokenValidated = PopulateAccountsClaim;
                });
            }
        }

        private Task PopulateAccountsClaim(TokenValidatedContext ctx)
        {
            var userId = ctx.Principal.Claims.First(c => c.Type.Equals(EmployerRecruitClaims.IdamsUserIdClaimTypeIdentifier)).Value;
            var accounts = _accountsSvc.GetAssociatedAccounts(userId);

            var accountsConcatenated = string.Join(",", accounts);
            var associatedAccountClaim = new Claim(EmployerRecruitClaims.AccountsClaimsTypeIdentifier, accountsConcatenated, ClaimValueTypes.String);

            ctx.Principal.Identities.First().AddClaim(associatedAccountClaim);

            return Task.CompletedTask;
        }

        private void ConfigureAuthorization(IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy("HasEmployerAccount", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim(EmployerRecruitClaims.AccountsClaimsTypeIdentifier);
                    policy.Requirements.Add(new EmployerAccountRequirement());
                });
            });

            services.AddSingleton<IAuthorizationHandler, EmployerAccountHandler>();
        }
    }
}
