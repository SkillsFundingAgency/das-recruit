using System;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Esfa.Recruit.Provider.Web.Middleware
{
    public class ProviderAccountHandler : AuthorizationHandler<ProviderAccountRequirement>
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IProviderVacancyClient _client;

        public ProviderAccountHandler(IHostingEnvironment hostingEnvironment, IProviderVacancyClient client)
        {
            _hostingEnvironment = hostingEnvironment;
            _client = client;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ProviderAccountRequirement requirement)
        {
            if (context.Resource is AuthorizationFilterContext mvcContext && mvcContext.RouteData.Values.ContainsKey(RouteValues.Ukprn))
            {
                if (context.User.HasClaim(c => c.Type.Equals(ProviderRecruitClaims.IdamsUserUkprnClaimsTypeIdentifier)))
                {
                    var ukprnFromUrl = mvcContext.RouteData.Values[RouteValues.Ukprn].ToString();

                    if (!string.IsNullOrEmpty(ukprnFromUrl))
                    {
                        mvcContext.HttpContext.Items.Add(ContextItemKeys.ProviderIdentifier, ukprnFromUrl);

                        await EnsureProviderIsSetup(mvcContext.HttpContext, long.Parse(ukprnFromUrl));

                        context.Succeed(requirement);
                    }
                }
            }
            else
            {
                context.Succeed(requirement);
            }
        }

        private async Task EnsureProviderIsSetup(HttpContext context, long ukprn)
        {
            await Task.CompletedTask;
            var key = string.Format(CookieNames.SetupProvider, ukprn);

            if (context.Request.Cookies[key] == null)
            {
                await _client.SetupProviderAsync(ukprn);
                context.Response.Cookies.Append(key, "1", EsfaCookieOptions.GetDefaultHttpCookieOption(_hostingEnvironment));
            }
        }
    }
}