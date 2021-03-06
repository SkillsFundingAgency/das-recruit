using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus;
using SFA.DAS.NServiceBus.Configuration;
using SFA.DAS.NServiceBus.Configuration.AzureServiceBus;
using SFA.DAS.NServiceBus.Configuration.NewtonsoftJsonSerializer;
using SFA.DAS.NServiceBus.Configuration.NLog;

namespace Esfa.Recruit.Vacancies.Jobs.NServiceBus
{
    public static class ConfigureNServiceBus
    {
        private const string RecruitVacanciesJobs = "SFA.Recruit.Vacancies.Jobs";

        public static void AddDasNServiceBus(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddSingleton(s =>
                {
                    var serviceBusConfiguration = new DasSharedNServiceBusConfiguration();
                    configuration.GetSection(nameof(DasSharedNServiceBusConfiguration)).Bind(serviceBusConfiguration);

                    var endpointConfiguration = new EndpointConfiguration(RecruitVacanciesJobs)
                        .UseAzureServiceBusTransport(serviceBusConfiguration.ConnectionString, r => { })
                        .UseErrorQueue($"{RecruitVacanciesJobs}-errors")
                        .UseInstallers()
                        .UseLicense(serviceBusConfiguration.NServiceBusLicense)
                        .UseMessageConventions()
                        .UseDasMessageConventions()
                        .UseNewtonsoftJsonSerializer()
                        .UseNLogFactory()
                        .UseServiceCollection(services);

                    return Endpoint.Start(endpointConfiguration).GetAwaiter().GetResult();
                })
                .AddHostedService<NServiceBusHostedService>();
        }
    }
}