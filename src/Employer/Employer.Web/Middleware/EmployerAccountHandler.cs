﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Esfa.Recruit.Employer.Web.Middleware
{
    public class EmployerAccountHandler : AuthorizationHandler<EmployerAccountRequirement>
    {
        private readonly IEmployerVacancyClient _client;

        public EmployerAccountHandler(IEmployerVacancyClient client)
        {
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
                        mvcContext.HttpContext.Items.Add(ContextItemKeys.EmployerIdentifier, accountIdFromUrl);

                        await EnsureEmployerIsSetup(mvcContext.HttpContext, accountIdFromUrl);
                        
                        context.Succeed(requirement);
                    }
                }
            }
        }

        private async Task EnsureEmployerIsSetup(HttpContext context, string employerAccountId)
        {
            var key = $"setup_employer_{employerAccountId}";
            if (context.Request.Cookies[key] == null)
            {
                await _client.SetupEmployer(employerAccountId);
                context.Response.Cookies.Append(key, String.Empty);
            }
        }
    }
}