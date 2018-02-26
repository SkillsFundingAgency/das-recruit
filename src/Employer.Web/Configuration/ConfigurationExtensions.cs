using Esfa.Recruit.Employer.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Middleware;

namespace Esfa.Recruit.Employer.Web.Configuration
{
    public static class ConfigurationExtensions
    {
        private const string HasEmployerAccountPolicyName = "HasEmployerAccount";

        public static void AddAuthorizationService(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy(HasEmployerAccountPolicyName, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim(EmployerRecruitClaims.AccountsClaimsTypeIdentifier);
                    policy.Requirements.Add(new EmployerAccountRequirement());
                });
            });

            services.AddSingleton<IAuthorizationHandler, EmployerAccountHandler>();
        }

        public static void AddMvcService(this IServiceCollection services, IHostingEnvironment hostingEnvironment, bool isAuthEnabled)
        {
            services.AddMvc(opts =>
            {
                if (!hostingEnvironment.IsDevelopment())
                {
                    opts.Filters.Add(new RequireHttpsAttribute());
                }

                if (!isAuthEnabled)
                {
                    opts.Filters.Add(new AllowAnonymousFilter());
                }
                else
                {
                    opts.Filters.Add(new AuthorizeFilter(HasEmployerAccountPolicyName));
                }

                var jsonInputFormatters = opts.InputFormatters.OfType<JsonInputFormatter>();
                foreach (var formatter in jsonInputFormatters)
                {
                    formatter.SupportedMediaTypes
                        .Add(MediaTypeHeaderValue.Parse("application/csp-report"));
                }

                opts.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            });
        }

        public static void AddAuthenticationService(this IServiceCollection services, AuthenticationConfiguration authConfig, IEmployerAccountService accountsSvc)
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

                options.Authority = authConfig.Authority;
                options.MetadataAddress = authConfig.MetaDataAddress;
                options.RequireHttpsMetadata = false;
                options.ResponseType = "code";
                options.ClientId = authConfig.ClientId;
                options.ClientSecret = authConfig.ClientSecret;
                options.Scope.Add("profile");

                options.Events.OnTokenValidated = async (ctx) => await PopulateAccountsClaim(ctx, accountsSvc);
            });
        }
        
        private static async Task PopulateAccountsClaim(Microsoft.AspNetCore.Authentication.OpenIdConnect.TokenValidatedContext ctx, IEmployerAccountService accountsSvc)
        {
            var userId = ctx.Principal.Claims.First(c => c.Type.Equals(EmployerRecruitClaims.IdamsUserIdClaimTypeIdentifier)).Value;
            var accounts = await accountsSvc.GetAccountIdentifiersAsync(userId);

            var accountsConcatenated = string.Join(",", accounts);
            var associatedAccountClaim = new Claim(EmployerRecruitClaims.AccountsClaimsTypeIdentifier, accountsConcatenated, ClaimValueTypes.String);

            ctx.Principal.Identities.First().AddClaim(associatedAccountClaim);
        }
    }
}
