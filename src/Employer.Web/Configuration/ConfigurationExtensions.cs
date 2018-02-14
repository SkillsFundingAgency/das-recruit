using Employer.Web.Middleware;
using Employer.Web.Services;
using Esfa.Recruit.Employer.Web;
using Esfa.Recruit.Employer.Web.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace Employer.Web.Configuration
{
    public static class ConfigurationExtensions
    {
        public static void AddAuthorizationService(this IServiceCollection services)
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
                    var policy = new AuthorizationPolicyBuilder()
                             .RequireAuthenticatedUser()
                             .Build();

                    opts.Filters.Add(new AuthorizeFilter(policy));

                    opts.Filters.Add(new AuthorizeFilter("HasEmployerAccount"));
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
    }
}
