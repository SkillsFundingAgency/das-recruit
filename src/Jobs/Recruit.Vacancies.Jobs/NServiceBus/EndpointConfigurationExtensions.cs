using Microsoft.Extensions.DependencyInjection;
using NServiceBus;

namespace Esfa.Recruit.Vacancies.Jobs.NServiceBus
{
    public static class EndpointConfigurationExtensions
    {
        public static EndpointConfiguration UseServiceCollection(this EndpointConfiguration config, IServiceCollection services)
        {
            config.UseContainer<ServicesBuilder>(c => c.ExistingServices(services));
            return config;
        }
        public static EndpointConfiguration UseDasMessageConventions(this EndpointConfiguration config)
        {
            var conventions = config.Conventions();
            conventions.DefiningEventsAs(t => 
                t.Namespace != null && (t.Namespace.StartsWith("SFA.DAS.EmployerAccounts.Messages.Events")));
            return config;
        }
    }
}