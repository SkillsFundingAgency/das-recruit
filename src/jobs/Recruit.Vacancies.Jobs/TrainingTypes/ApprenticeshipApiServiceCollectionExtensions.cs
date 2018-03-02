using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Apprenticeships.Api.Client;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ApprenticeshipApiServiceCollectionExtensions
    {
        public static IServiceCollection AddApprentieshipsApi(this IServiceCollection services)
        {
            services.AddScoped<IStandardApiClient, StandardApiClient>();
            services.AddScoped<IFrameworkApiClient, FrameworkApiClient>();

            return services;
        }
    }
}
