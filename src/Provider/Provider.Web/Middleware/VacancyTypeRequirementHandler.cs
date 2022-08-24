using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Microsoft.AspNetCore.Authorization;

namespace Esfa.Recruit.Provider.Web.Middleware
{
    public class VacancyTypeRequirementHandler : AuthorizationHandler<VacancyTypeRequirement>
    {
        private readonly ServiceParameters _serviceParameters;

        public VacancyTypeRequirementHandler(ServiceParameters serviceParameters)
        {
            _serviceParameters = serviceParameters;
        }
        
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, VacancyTypeRequirement requirement)
        {
            if (_serviceParameters.VacancyType == requirement.VacancyType)
            {
                context.Succeed(requirement);                
            }
            else
            {
                context.Fail();
            }
            return Task.CompletedTask;
        }
    }
}