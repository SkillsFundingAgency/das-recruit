using Employer.Web.Services;
using Esfa.Recruit.Employer.Web.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Employer.Web.Configuration
{
    public static class ConfigurationExtensions
    {
        public static void AddVacancyAuthentication(this IServiceCollection services, AuthenticationConfiguration options, IGetAssociatedEmployerAccountsService svc)
        {
            
        }
    }
}
