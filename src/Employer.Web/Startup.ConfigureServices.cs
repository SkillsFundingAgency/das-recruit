using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Employer.Web.Configuration;
using Employer.Web.Middleware;
using Employer.Web.Services;
using Esfa.Recruit.Employer.Web.Configuration;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

namespace Esfa.Recruit.Employer.Web
{
    public partial class Startup
    {
        private readonly bool _isAuthEnabled = true;
        private IConfiguration _configuration { get; }
        private IHostingEnvironment _hostingEnvironment { get; }
        private AuthenticationConfiguration _authConfig { get; }

        private readonly IGetAssociatedEmployerAccountsService _accountsSvc;

        public Startup(IConfiguration config, IHostingEnvironment env)
        {
            _configuration = config;
            _hostingEnvironment = env;
            _authConfig = _configuration.GetSection("Authentication").Get<AuthenticationConfiguration>();

            _accountsSvc = new GetAssociatedEmployerAccountsService();

            if (env.IsDevelopment()  && _authConfig.IsEnabledForDev == false)
            {
                _isAuthEnabled = false;
            }
        }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddIoC(_configuration);

            // Routing has to come before adding Mvc
            services.AddRouting(opt =>
            {
                opt.LowercaseUrls = true;
                opt.AppendTrailingSlash = true;
            });

            services.AddMvc(opts =>
            {
                //opts.Conventions.Insert(0, new EmployerAccountRoutePrefixConvention());

                if (!_hostingEnvironment.IsDevelopment())
                {
                    opts.Filters.Add(new RequireHttpsAttribute());
                }

                if (!_isAuthEnabled)
                {
                    opts.Filters.Add(new AllowAnonymousFilter());
                }

                var policy = new AuthorizationPolicyBuilder()
                         .RequireAuthenticatedUser()
                         .Build();

                opts.Filters.Add(new AuthorizeFilter(policy));

                opts.Filters.Add(new AuthorizeFilter("HasEmployerAccount"));

                var jsonInputFormatters = opts.InputFormatters.OfType<JsonInputFormatter>();
                    foreach (var formatter in jsonInputFormatters)
                {
                    formatter.SupportedMediaTypes
                        .Add(MediaTypeHeaderValue.Parse("application/csp-report"));
                }

                opts.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            });

            services.AddApplicationInsightsTelemetry(_configuration);
            
            ConfigureAuthentication(services);
            ConfigureAuthorization(services);

        }

        private void ConfigureAuthentication(IServiceCollection services)
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
