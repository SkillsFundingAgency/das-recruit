using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs
{
    class Program
    {
        private static string _environmentName;
        private static bool _isDevelopment;

        static Program()
        {
            _environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            _isDevelopment = _environmentName?.Equals("Development", StringComparison.CurrentCultureIgnoreCase) ?? false;
        }

        static void Main(string[] args)
        {
            ILoggerFactory loggerFactory = null;

            try
            {
                IServiceCollection serviceCollection = new ServiceCollection();
                var serviceProvider = ConfigureServices(serviceCollection).BuildServiceProvider();

                var configuration = BuildConfiguration();
                
                using (BuildLoggerFactory(serviceProvider, configuration))
                {
                    JobHostConfiguration jobConfiguration = GetHostConfiguration(serviceProvider);

                    var host = new JobHost(jobConfiguration);
                    host.RunAndBlock();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                loggerFactory.Dispose();
            }
        }

        private static ILoggerFactory BuildLoggerFactory(ServiceProvider serviceProvider, IConfigurationRoot config)
        {
            var instrumentationKey = config["AppInsights_InstrumentationKey"];
            Console.WriteLine($"AppInsights: {config.GetValue<string>("AppInsights_InstrumentationKey")}");
            
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            loggerFactory.AddNLog(new NLogProviderOptions { CaptureMessageProperties = true, CaptureMessageTemplates = true });
            loggerFactory.ConfigureNLog("nlog.config");
            loggerFactory.AddApplicationInsights(instrumentationKey, null);

            return loggerFactory;
        }

        private static JobHostConfiguration GetHostConfiguration(ServiceProvider serviceProvider)
        {
            // Host configuration
            var jobConfiguration = new JobHostConfiguration();
            jobConfiguration.Queues.MaxPollingInterval = TimeSpan.FromSeconds(10);
            jobConfiguration.Queues.BatchSize = 1;
            jobConfiguration.JobActivator = new CustomJobActivator(serviceProvider);
            jobConfiguration.UseTimers();

            if (_isDevelopment)
            {
                jobConfiguration.DashboardConnectionString = null; // Reduces errors in output.
                jobConfiguration.UseDevelopmentSettings();
                jobConfiguration.Tracing.ConsoleLevel = TraceLevel.Off;
            }

            return jobConfiguration;
        }

        private static IConfigurationRoot BuildConfiguration()
        {
            // Setup configuration classes
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appSettings.json", optional: false)
                .AddJsonFile($"appSettings.{_environmentName}.json", true)
                .AddEnvironmentVariables();

            if (_isDevelopment)
            {
                builder.AddUserSecrets<Program>();
            }

            var configuration = builder.Build();
            Environment.SetEnvironmentVariable("AzureWebJobsDashboard", configuration.GetConnectionString("WebJobsDashboard"));
            Environment.SetEnvironmentVariable("AzureWebJobsStorage", configuration.GetConnectionString("WebJobsStorage"));  

            return configuration;
        }

        private static IServiceCollection ConfigureServices(IServiceCollection serviceCollection)
        {
            // Setup dependencies
            serviceCollection.AddSingleton<ILoggerFactory, LoggerFactory>();
            serviceCollection.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            serviceCollection.AddLogging((options) => 
            {
                options.SetMinimumLevel(LogLevel.Trace);
                options.AddConsole();
                options.AddDebug();
            });

            // Add Jobs
            serviceCollection.AddScoped<GenerateVacancyNumberJob, GenerateVacancyNumberJob>();
            //serviceCollection.AddSingleton<IApprenticeshipProgrammeApiClient, ApprenticeshipProgrammeApiClient>();

            return serviceCollection;
        }
    }
}
