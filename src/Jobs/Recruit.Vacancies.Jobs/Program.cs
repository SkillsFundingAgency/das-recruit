using System;
using System.Diagnostics;
using System.IO;
using Esfa.Recruit.Vacancies.Jobs.ApprenticeshipProgrammes;
using Esfa.Recruit.Vacancies.Jobs.EditVacancyInfo;
using Esfa.Recruit.Vacancies.Jobs.GenerateVacancyNumber;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using SFA.DAS.Apprenticeships.Api.Client;

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
            ILogger logger = null;

            try
            {
                var configuration = BuildConfiguration();

                IServiceCollection serviceCollection = new ServiceCollection();
                var serviceProvider = ConfigureServices(serviceCollection, configuration).BuildServiceProvider();

                var factory = ConfigureLoggingFactory(serviceProvider, configuration);

                logger = factory.CreateLogger("Program");

                JobHostConfiguration jobConfiguration = GetHostConfiguration(serviceProvider);
                var host = new JobHost(jobConfiguration);

                var cancellationToken = new WebJobsShutdownWatcher().Token;
                cancellationToken.Register(host.Stop);

                logger.LogInformation("Job Starting");

                host.RunAndBlock();

                logger.LogInformation("Job Stopping");
            }
            catch (Exception ex)
            {
                if (logger != null)
                {
                    logger.LogCritical(ex, "The Job has met with a horrible end!!");
                }

                throw;
            }
            finally
            {
                NLog.LogManager.Shutdown(); 
            }
        }

        private static ILoggerFactory ConfigureLoggingFactory(ServiceProvider serviceProvider, IConfigurationRoot config)
        {
            var instrumentationKey = config["AppInsights_InstrumentationKey"];
            
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            loggerFactory.AddNLog(new NLogProviderOptions { CaptureMessageProperties = true, CaptureMessageTemplates = true });
            NLog.LogManager.LoadConfiguration("nlog.config");
			
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
            Environment.SetEnvironmentVariable("EventQueueConnectionString", configuration.GetConnectionString("Storage"));

            return configuration;
        }

        private static IServiceCollection ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // Setup dependencies
            services.AddSingleton<ILoggerFactory, LoggerFactory>();
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            services.AddLogging((options) => 
            {
                options.AddConfiguration(configuration.GetSection("Logging"));
                options.SetMinimumLevel(LogLevel.Trace);
                options.AddConsole();
                options.AddDebug();
            });

            services.AddScoped<GenerateVacancyNumberUpdater>();
            services.AddScoped<ApprenticeshipProgrammesUpdater>();
            services.AddScoped<EditVacancyInfoUpdater>();

            services.AddSingleton<IApprenticeshipProgrammeApiClient, ApprenticeshipProgrammeApiClient>();
            services.AddApprentieshipsApi(configuration);
            services.AddRecruitStorageClient(configuration);

            // Add Jobs
            services.AddScoped<GenerateVacancyNumberJob>();
            services.AddScoped<ApprenticeshipProgrammesJob>();
            services.AddScoped<EditVacancyInfoJob>();

            return services;
        }
    }
}
