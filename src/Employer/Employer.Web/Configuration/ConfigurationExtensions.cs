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
using Microsoft.Extensions.Logging;
using Esfa.Recruit.Employer.Web.Filters;
using Esfa.Recruit.Employer.Web.Interfaces;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Hosting;

namespace Esfa.Recruit.Employer.Web.Configuration
{
    public static class ConfigurationExtensions
    {
        public static void AddAuthorizationService(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddTransient<IEmployerAccountAuthorizationHandler, EmployerAccountAuthorizationHandler>();
            services.AddSingleton<IAuthorizationHandler, EmployerAccountOwnerOrTransactorAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, EmployerAccountHandler>();
            services.AddAuthorization(options =>
            {
                // default authorization policy for all controller actions.
                options.AddPolicy(
                    PolicyNames.HasEmployerAccountPolicyName, policy =>
                    {
                        policy.RequireClaim(EmployerRecruitClaims.AccountsClaimsTypeIdentifier);
                        policy.Requirements.Add(new EmployerAccountRequirement());
                        policy.RequireAuthenticatedUser();
                    });
                // authorization policy for controller actions more specific for admin/owner roles.
                options.AddPolicy(
                    PolicyNames.HasEmployerOwnerOrTransactorAccount, policy =>
                    {
                        policy.RequireClaim(EmployerRecruitClaims.AccountsClaimsTypeIdentifier);
                        policy.Requirements.Add(new EmployerAccountOwnerOrTransactorRequirement());
                        policy.RequireAuthenticatedUser();
                    });
            });
        }

        public static void AddMvcService(this IServiceCollection services, IWebHostEnvironment hostingEnvironment, bool isAuthEnabled, ILoggerFactory loggerFactory)
        {
            services.AddAntiforgery(options =>
            {
                options.Cookie.Name = CookieNames.AntiForgeryCookie;
                options.FormFieldName = "_csrfToken";
                options.HeaderName = "X-XSRF-TOKEN";
            });
            services.Configure<CookieTempDataProviderOptions>(options => options.Cookie.Name = CookieNames.RecruitTempData);
            services.AddSingleton<ITempDataProvider, CookieTempDataProvider>();
            services.AddMvc(opts =>
                {
                    opts.EnableEndpointRouting = false;
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
                        opts.Filters.Add(new AuthorizeFilter(PolicyNames.HasEmployerAccountPolicyName));
                    }

                    var jsonInputFormatters = opts.InputFormatters.OfType<NewtonsoftJsonInputFormatter>();
                    foreach (var formatter in jsonInputFormatters)
                    {
                        formatter.SupportedMediaTypes
                            .Add(MediaTypeHeaderValue.Parse("application/csp-report"));
                    }

                    opts.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());

                    opts.Filters.AddService<PlannedOutageResultFilter>();
                    opts.Filters.AddService<GoogleAnalyticsFilter>();
                    opts.Filters.AddService<ZendeskApiFilter>();
                    opts.AddTrimModelBinderProvider(loggerFactory);
                })
                .AddNewtonsoftJson();
            services.AddFluentValidationAutoValidation();
        }

        public static void AddAuthenticationService(this IServiceCollection services, AuthenticationConfiguration authConfig)
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
                options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
                options.SlidingExpiration = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(AuthenticationConfiguration.SessionTimeoutMinutes);
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
                options.UsePkce = false;
                
                options.Events.OnRemoteFailure = ctx =>
                {
                    if (ctx.Failure.Message.Contains("Correlation failed"))
                    {
                        var logger = services.BuildServiceProvider().GetRequiredService<ILoggerFactory>().CreateLogger<Startup>();
                        logger.LogDebug("Correlation Cookie was invalid - probably timed-out");

                        ctx.Response.Redirect("/");
                        ctx.HandleResponse();
                    }

                    return Task.CompletedTask;
                };
            });
            services
                .AddOptions<OpenIdConnectOptions>("oidc")
                .Configure<IRecruitVacancyClient>((options, recruitVacancyClient) =>
                {
                    options.Events.OnTokenValidated = async (ctx) =>
                    {
                        await PopulateAccountsClaim(ctx, recruitVacancyClient);
                        await HandleUserSignedIn(ctx, recruitVacancyClient);
                    };
                });
        }

        private static async Task PopulateAccountsClaim(
            Microsoft.AspNetCore.Authentication.OpenIdConnect.TokenValidatedContext ctx, 
            IRecruitVacancyClient vacancyClient)
        {
            var userId = ctx.Principal.GetUserId();
            var email = ctx.Principal.GetEmailAddress();
            var accounts = await vacancyClient.GetEmployerIdentifiersAsync(userId, email);
            var accountsAsJson = JsonConvert.SerializeObject(accounts.UserAccounts.ToDictionary(k => k.AccountId));
            
            var associatedAccountsClaim = new Claim(EmployerRecruitClaims.AccountsClaimsTypeIdentifier, accountsAsJson, JsonClaimValueTypes.Json);

            ctx.Principal.Identities.First().AddClaim(associatedAccountsClaim);
        }

        private static Task HandleUserSignedIn(Microsoft.AspNetCore.Authentication.OpenIdConnect.TokenValidatedContext ctx, IRecruitVacancyClient vacancyClient)
        {
            var user = ctx.Principal.ToVacancyUser();
            return vacancyClient.UserSignedInAsync(user, UserType.Employer);
        }


    }
}