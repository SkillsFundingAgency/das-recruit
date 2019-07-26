using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus;
using SFA.DAS.NServiceBus;
using SFA.DAS.NServiceBus.AzureServiceBus;
using SFA.DAS.NServiceBus.NewtonsoftJsonSerializer;
using SFA.DAS.NServiceBus.NLog;

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
                    var hostingEnvironment = s.GetService<IHostingEnvironment>();

                    var serviceBusConfiguration = new DasSharedNServiceBusConfiguration();
                    configuration.GetSection(nameof(DasSharedNServiceBusConfiguration)).Bind(serviceBusConfiguration);

                    var endpointConfiguration = new EndpointConfiguration(RecruitVacanciesJobs)
                        .UseAzureServiceBusTransport(false,
                            () => serviceBusConfiguration.ConnectionString, r => { })
                        .UseErrorQueue()
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