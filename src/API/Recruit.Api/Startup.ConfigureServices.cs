using System.Collections.Generic;
using System.Text.Json.Serialization;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;
using Esfa.Recruit.Vacancies.Client.Ioc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Logs;
using SFA.DAS.Api.Common.AppStart;
using SFA.DAS.Api.Common.Configuration;
using SFA.DAS.Api.Common.Infrastructure;
using SFA.DAS.Encoding;
using SFA.DAS.Recruit.Api.Commands;
using SFA.DAS.Recruit.Api.Configuration;
using SFA.DAS.Recruit.Api.Mappers;
using SFA.DAS.Recruit.Api.Services;

namespace SFA.DAS.Recruit.Api;

public partial class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddOptions();
        services.Configure<ConnectionStrings>(_configuration.GetSection("ConnectionStrings"));
        services.Configure<RecruitConfiguration>(_configuration.GetSection("Recruit"));
        services.Configure<AzureActiveDirectoryConfiguration>(_configuration.GetSection("AzureAd"));

        services.AddLogging(builder =>
        {
            builder.AddFilter<OpenTelemetryLoggerProvider>(string.Empty, LogLevel.Information);
            builder.AddFilter<OpenTelemetryLoggerProvider>("Microsoft", LogLevel.Information);
        });

        services.AddScoped(_ => new ServiceParameters());

        var azureAdConfig = _configuration
            .GetSection("AzureAd")
            .Get<AzureActiveDirectoryConfiguration>();

        var policies = new Dictionary<string, string>
        {
            { PolicyNames.Default, "Default" },
        };

        services.AddAuthentication(azureAdConfig, policies);

        services.AddMediatR(configuration => configuration.RegisterServicesFromAssembly(typeof(CreateVacancyCommand).Assembly));

        services.AddSingleton<IVacancySummaryMapper, VacancySummaryMapper>();
        services.AddSingleton<IQueryStoreReader, QueryStoreClient>();
        services.AddSingleton<IFeature, Feature>();
        RegisterDasEncodingService(services, _configuration);

        services.AddRecruitStorageClient(_configuration);

        MongoDbConventions.RegisterMongoConventions();

        services.AddHealthChecks()
            .AddMongoDb(_configuration.GetConnectionString("MongoDb"))
            .AddApplicationInsightsPublisher();

        services.AddApplicationInsightsTelemetry(_configuration);

        if (!string.IsNullOrEmpty(_configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]!))
        {
            // This service will collect and send telemetry data to Azure Monitor.
            services.AddOpenTelemetry().UseAzureMonitor(options =>
            {
                options.ConnectionString = _configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]!;
            });
        }

        services
            .AddMvc(o =>
            {
                if (!_hostingEnvironment.IsDevelopment())
                {
                    o.Conventions.Add(new AuthorizeControllerModelConvention(new List<string>()));
                }

                o.Conventions.Add(new ApiExplorerGroupPerVersionConvention());
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "RecruitAPI", Version = "v1" });
        });
    }

    private static void RegisterDasEncodingService(IServiceCollection services, IConfiguration configuration)
    {
        var dasEncodingConfig = new EncodingConfig { Encodings = [] };
        configuration.GetSection(nameof(dasEncodingConfig.Encodings)).Bind(dasEncodingConfig.Encodings);
        services.AddSingleton(dasEncodingConfig);
        services.AddSingleton<IEncodingService, EncodingService>();
    }
}