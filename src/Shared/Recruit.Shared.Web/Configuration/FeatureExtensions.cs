using Microsoft.Extensions.DependencyInjection;

namespace Esfa.Recruit.Shared.Configuration
{
    public static class FeatureExtensions
    {
        public static void AddFeatureToggle(this IServiceCollection services)
        {
            services.AddSingleton<IFeature, Feature>();
        }
    }
}
