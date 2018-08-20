using System;
using System.Diagnostics;
using System.IO;
using Esfa.Recruit.Vacancies.Client.Application.CommandHandlers;
using Esfa.Recruit.Vacancies.Jobs.ApprenticeshipProgrammes;
using Esfa.Recruit.Vacancies.Jobs.BankHoliday;
using Esfa.Recruit.Vacancies.Jobs.DashboardGenerator;
using Esfa.Recruit.Vacancies.Jobs.EditVacancyInfo;
using Esfa.Recruit.Vacancies.Jobs.QaDashboard;
using Esfa.Recruit.Vacancies.Jobs.LiveVacanciesGenerator;
using Esfa.Recruit.Vacancies.Jobs.VacancyApplication;
using Esfa.Recruit.Vacancies.Jobs.VacancyEvents;
using Esfa.Recruit.Vacancies.Jobs.VacancyReviewEvents;
using Esfa.Recruit.Vacancies.Jobs.VacancyStatus;
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
        private static readonly string EnvironmentName;
        private static readonly bool IsDevelopment;

        static Program()
        {
            EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            IsDevelopment = EnvironmentName?.Equals("Development", StringComparison.CurrentCultureIgnoreCase) ?? false;
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

                JobHostConfiguration jobConfiguration = GetHostConfiguration(serviceProvider, factory);
                var host = new JobHost(jobConfiguration);

                var cancellationToken = new WebJobsShutdownWatcher().Token;
                cancellationToken.Register(host.Stop);

                logger.LogInformation("Job Starting");

                host.RunAndBlock();

                logger.LogInformation("Job Stopping");
            }
            catch (Exception ex)
            {
                logger?.LogCritical(ex, "The Job has met with a horrible end!!");

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

        private static JobHostConfiguration GetHostConfiguration(ServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            // Host configuration
            var jobConfiguration = new JobHostConfiguration();
            jobConfiguration.Queues.MaxPollingInterval = TimeSpan.FromSeconds(10);
            jobConfiguration.Queues.BatchSize = 1;
            jobConfiguration.Queues.QueueProcessorFactory = new CustomQueueProcessorFactory();
            jobConfiguration.JobActivator = new CustomJobActivator(serviceProvider);
            jobConfiguration.UseTimers();
            jobConfiguration.LoggerFactory = loggerFactory;
            
            if (IsDevelopment)
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
                .AddJsonFile($"appSettings.{EnvironmentName}.json", true)
                .AddEnvironmentVariables();

            if (IsDevelopment)
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

            services.AddScoped<VacancyEventHandler>();
            services.AddScoped<VacancyReviewEventHandler>();
            services.AddScoped<ApprenticeshipProgrammesUpdater>();
            services.AddScoped<EditVacancyInfoUpdater>();
            services.AddScoped<LiveVacancyStatusInspector>();
            services.AddScoped<DashboardCreator>();
            services.AddScoped<CreateApplicationReviewCommandHandler>();
            services.AddScoped<LiveVacanciesCreator>();

            services.AddSingleton<IApprenticeshipProgrammeApiClient, ApprenticeshipProgrammeApiClient>();
            services.AddApprentieshipsApi(configuration);
            services.AddRecruitStorageClient(configuration);

            // Add Jobs
            services.AddScoped<VacancyEventsJob>();
            services.AddScoped<VacancyReviewEventsJob>();
            services.AddScoped<ApprenticeshipProgrammesJob>();
            services.AddScoped<EditVacancyInfoJob>();
            services.AddScoped<VacancyStatusJob>();
            services.AddScoped<DashboardGeneratorJob>();
            services.AddScoped<VacancyApplicationJob>();
            services.AddScoped<LiveVacanciesGeneratorJob>();
            services.AddScoped<BankHolidayJob>();
            services.AddScoped<QaDashboardJob>();

            return services;
        }
    }
}
