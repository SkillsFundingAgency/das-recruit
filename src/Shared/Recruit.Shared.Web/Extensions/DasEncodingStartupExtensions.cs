using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Encoding;

namespace Esfa.Recruit.Shared.Web.Extensions
{
    public static class DasEncodingStartupExtensions
    {
        public static IServiceCollection AddDasEncoding(this IServiceCollection services, IConfiguration configuration)
        {
            var dasEncodingConfig = new EncodingConfig { Encodings = [] };
            configuration.GetSection(nameof(dasEncodingConfig.Encodings)).Bind(dasEncodingConfig.Encodings);
            services.AddSingleton(dasEncodingConfig);
            services.AddSingleton<IEncodingService, EncodingService>();
            
            return services;
        }
    }
}
