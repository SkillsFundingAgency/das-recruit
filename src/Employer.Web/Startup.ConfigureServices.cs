using Employer.Web.Configuration;
using Employer.Web.Services;
using Esfa.Recruit.Employer.Web.Configuration;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;

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
            
            services.AddMvcService(_hostingEnvironment, _isAuthEnabled);

            services.AddApplicationInsightsTelemetry(_configuration);

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
            
            services.AddAuthorizationService();
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
    }
}
