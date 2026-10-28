using Azure.Monitor.OpenTelemetry.AspNetCore;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;
using Esfa.Recruit.Vacancies.Client.Ioc;
using Esfa.Recruit.Vacancies.Jobs.Triggers.QueueTriggers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Encoding;

namespace Esfa.Recruit.Vacancies.Jobs;

internal static class ServiceCollectionExtensions
{
    public static void ConfigureJobServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddRecruitStorageClient(configuration);

        // Add Jobs
        services.AddScoped<DomainEventsQueueTrigger>();
        services.AddScoped<VacancyStatusQueueTrigger>();
        services.AddScoped<UpdateProvidersQueueTrigger>();
            
        // Domain Event Queue Handlers

        RegisterDasEncodingService(services, configuration);

        services.AddSingleton<IFeature, Feature>();
    }

    public static void AddOpenTelemetryRegistration(this IServiceCollection services, string appInsightsConnectionString)
    {
        if (!string.IsNullOrEmpty(appInsightsConnectionString))
        {
            // This service will collect and send telemetry data to Azure Monitor.
            services.AddOpenTelemetry().UseAzureMonitor(options =>
            {
                options.ConnectionString = appInsightsConnectionString;
            });
        }
    }
        
    private static void RegisterDasEncodingService(IServiceCollection services, IConfiguration configuration)
    {
        var dasEncodingConfig = new EncodingConfig { Encodings = []};
        configuration.GetSection(nameof(dasEncodingConfig.Encodings)).Bind(dasEncodingConfig.Encodings);
        services.AddSingleton(dasEncodingConfig);
        services.AddSingleton<IEncodingService, EncodingService>();
    }
}
