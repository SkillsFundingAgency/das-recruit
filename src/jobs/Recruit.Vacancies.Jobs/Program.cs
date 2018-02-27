using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Esfa.Recruit.Vacancies.Jobs
{
    class Program
    {
        private static string _environmentName;
        private static bool _isDevelopment;

        Program()
        {
            _environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            _isDevelopment = _environmentName?.Equals("Development", StringComparison.CurrentCultureIgnoreCase) ?? false;
        }

        static void Main(string[] args)
        {
            // TODO: LWA - try catch and logging
            try
            {
                IServiceCollection serviceCollection = new ServiceCollection();
                ConfigureServices(serviceCollection);

                var jobConfiguration = new JobHostConfiguration();
                jobConfiguration.Queues.MaxPollingInterval = TimeSpan.FromSeconds(10);
                jobConfiguration.Queues.BatchSize = 1;
                jobConfiguration.JobActivator = new CustomJobActivator(serviceCollection.BuildServiceProvider());
                jobConfiguration.UseTimers();

                if (_isDevelopment)
                {
                    jobConfiguration.UseDevelopmentSettings();
                }

                var host = new JobHost(jobConfiguration);
                host.RunAndBlock();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static IConfiguration BuildConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appSettings.json", optional: false)
                .AddJsonFile($"appSettings.{_environmentName}.json", true)
                .AddUserSecrets<Program>()
                .Build();
        }

        private static void ConfigureServices(IServiceCollection serviceCollection)
        {
            // Setup configuration classes
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appSettings.json", optional: false)
                .AddJsonFile($"appSettings.{_environmentName}.json", true)
                .AddUserSecrets<Program>()
                .Build();

            // Setup dependencies

            // Setup Jobs
            serviceCollection.AddScoped<GenerateVacancyNumberJob, GenerateVacancyNumberJob>();
            
            Environment.SetEnvironmentVariable("AzureWebJobsDashboard", configuration.GetConnectionString("WebJobsDashboard"));
            Environment.SetEnvironmentVariable("AzureWebJobsStorage", configuration.GetConnectionString("WebJobsStorage"));  
               
        }
    }
}
