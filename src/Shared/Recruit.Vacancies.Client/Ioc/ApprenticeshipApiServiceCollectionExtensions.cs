using Microsoft.Extensions.Configuration;
using SFA.DAS.Apprenticeships.Api.Client;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ApprenticeshipApiServiceCollectionExtensions
    {
        public static IServiceCollection AddApprenticeshipsApi(this IServiceCollection services, IConfiguration configuration)
        {
            var baseUrl = configuration.GetValue<string>("ApprenticeshipProgrammesApiBaseUrl");

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                services.AddScoped<IStandardApiClient, StandardApiClient>();
                services.AddScoped<IFrameworkApiClient, FrameworkApiClient>();
            }
            else
            {
                services.AddScoped<IStandardApiClient, StandardApiClient>(s => new StandardApiClient(baseUrl));
                services.AddScoped<IFrameworkApiClient, FrameworkApiClient>(s => new FrameworkApiClient(baseUrl));
            }
            
            return services;
        }
    }
}
