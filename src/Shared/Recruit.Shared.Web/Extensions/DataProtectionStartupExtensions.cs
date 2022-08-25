using Esfa.Recruit.Shared.Web.Configuration;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Microsoft.Extensions.Hosting;

namespace Esfa.Recruit.Shared.Web.Extensions
{
    public static class DataProtectionStartupExtensions
    {
        public static IServiceCollection AddDataProtection(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment, string applicationName)
        {
            if (!environment.IsDevelopment())
            {
                var redisConfiguration = configuration.GetSection("RedisConnectionSettings")
                    .Get<RedisConnectionSettings>();

                if (redisConfiguration != null)
                {
                    var redisConnectionString = redisConfiguration.RedisConnectionString;
                    var dataProtectionKeysDatabase = redisConfiguration.DataProtectionKeysDatabase;

                    var redis = ConnectionMultiplexer
                        .Connect($"{redisConnectionString},{dataProtectionKeysDatabase}");

                    services.AddDataProtection()
                        .SetApplicationName(applicationName)
                        .PersistKeysToStackExchangeRedis(redis, "DataProtection-Keys");
                }
            }

            return services;
        }
    }
}
