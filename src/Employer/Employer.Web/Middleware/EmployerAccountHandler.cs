using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Esfa.Recruit.Employer.Web.Middleware
{
    public class EmployerAccountHandler : AuthorizationHandler<EmployerAccountRequirement>
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IEmployerVacancyClient _client;

        public EmployerAccountHandler(IWebHostEnvironment hostingEnvironment, IEmployerVacancyClient client)
        {
            _hostingEnvironment = hostingEnvironment;
            _client = client;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, EmployerAccountRequirement requirement)
        {
            if (context.Resource is AuthorizationFilterContext mvcContext && mvcContext.RouteData.Values.ContainsKey(RouteValues.EmployerAccountId))
            {
                if (context.User.HasClaim(c => c.Type.Equals(EmployerRecruitClaims.AccountsClaimsTypeIdentifier)))
                {
                    var accountIdFromUrl = mvcContext.RouteData.Values[RouteValues.EmployerAccountId].ToString().ToUpper();
                    var employerAccounts = context.User.GetEmployerAccounts();

                    if (employerAccounts.Contains(accountIdFromUrl))
                    {
                        if (!mvcContext.HttpContext.Items.ContainsKey(ContextItemKeys.EmployerIdentifier))
                        {
                            mvcContext.HttpContext.Items.Add(ContextItemKeys.EmployerIdentifier, accountIdFromUrl);
                        }

                        await EnsureEmployerIsSetup(mvcContext.HttpContext, accountIdFromUrl);
                        
                        context.Succeed(requirement);
                    }
                }
            }
            else
            {
                context.Succeed(requirement);
            }
        }

        private async Task EnsureEmployerIsSetup(HttpContext context, string employerAccountId)
        {
            var key = string.Format(CookieNames.SetupEmployer, employerAccountId);

            if (context.Request.Cookies[key] == null)
            {
                await _client.SetupEmployerAsync(employerAccountId);
                context.Response.Cookies.Append(key, "1", EsfaCookieOptions.GetDefaultHttpCookieOption(_hostingEnvironment));
            }
        }
    }
}