using Esfa.Recruit.Shared.Web.FeatureToggle;
using Microsoft.Extensions.DependencyInjection;

namespace Esfa.Recruit.Shared.Web.Configuration
{
    public static class FeatureExtensions
    {
        public static void AddFeatureToggle(this IServiceCollection services)
        {
            services.AddSingleton<IFeature, Feature>();
        }
    }
}
