using Microsoft.Extensions.DependencyInjection;

namespace Communication.Core
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddCommunication(this IServiceCollection services)
        {
            services.AddTransient<ICommunicationProcessor, CommunicationProcessor>();
            services.AddTransient<ICommunicationService, CommunicationService>();
            return services;
        }
    }
}