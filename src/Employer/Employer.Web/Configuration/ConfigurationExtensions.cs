using System;
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
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Middleware;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;

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

            services.AddTransient<IAuthorizationHandler, EmployerAccountHandler>();
        }

        public static void AddMvcService(this IServiceCollection services, IHostingEnvironment hostingEnvironment, bool isAuthEnabled, ILoggerFactory loggerFactory)
        {
            services.AddAntiforgery(options =>
            {
                options.Cookie.Name = CookieNames.AntiForgeryCookie;
                options.FormFieldName = "_csrfToken";
                options.HeaderName = "X-XSRF-TOKEN";
            });
            services.Configure<CookieTempDataProviderOptions>(options => options.Cookie.Name = CookieNames.RecruitTempData);
            services.AddSingleton<ITempDataProvider, CookieTempDataProvider>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

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

                opts.AddTrimModelBinderProvider(loggerFactory);
            })
            .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<Startup>())
            .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        public static void AddAuthenticationService(this IServiceCollection services, AuthenticationConfiguration authConfig, IEmployerVacancyClient vacancyClient, IHostingEnvironment hostingEnvironment)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "Cookies";
                options.DefaultChallengeScheme = "oidc";
            })
            .AddCookie("Cookies", options =>
            {
                options.Cookie.Name = CookieNames.RecruitData;

                if (!hostingEnvironment.IsDevelopment())
                {
                    options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
                    options.SlidingExpiration = true;
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(AuthenticationConfiguration.SessionTimeoutMinutes);
                }

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

                options.Events.OnTokenValidated = async (ctx) =>
                {
                    await PopulateAccountsClaim(ctx, vacancyClient);
                    await HandleUserSignedIn(ctx, vacancyClient);
                };
            });
        }
        
        private static async Task PopulateAccountsClaim(Microsoft.AspNetCore.Authentication.OpenIdConnect.TokenValidatedContext ctx, IEmployerVacancyClient vacancyClient)
        {
            var userId = ctx.Principal.GetUserId();
            var accounts = await vacancyClient.GetEmployerIdentifiersAsync(userId);
            var accountsAsJson = JsonConvert.SerializeObject(accounts);
            var associatedAccountsClaim = new Claim(EmployerRecruitClaims.AccountsClaimsTypeIdentifier, accountsAsJson, JsonClaimValueTypes.Json);

            ctx.Principal.Identities.First().AddClaim(associatedAccountsClaim);
        }

        private static Task HandleUserSignedIn(Microsoft.AspNetCore.Authentication.OpenIdConnect.TokenValidatedContext ctx, IEmployerVacancyClient vacancyClient)
        {
            var user = ctx.Principal.ToVacancyUser();
            return vacancyClient.UserSignedInAsync(user);
        }
    }
}