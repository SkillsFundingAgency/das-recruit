using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus;

namespace Esfa.Recruit.Vacancies.Jobs.NServiceBus
{
    public static class EndpointConfigurationExtensions
    {
        private static readonly string[] _recognisedEventMessageNamespaces = new string[]
        {
            "SFA.DAS.EmployerAccounts.Messages.Events",
            "SFA.DAS.ProviderRelationships.Messages"
        };

        public static EndpointConfiguration UseServiceCollection(this EndpointConfiguration config, IServiceCollection services)
        {
            config.UseContainer<ServicesBuilder>(c => c.ExistingServices(services));
            return config;
        }

        public static EndpointConfiguration UseDasMessageConventions(this EndpointConfiguration config)
        {
            var conventions = config.Conventions();
            conventions.DefiningEventsAs(t =>
                t.Namespace != null && _recognisedEventMessageNamespaces.Any(nsPrefix => t.Namespace.StartsWith(nsPrefix)));
            return config;
        }
    }
}