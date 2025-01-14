using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using SFA.DAS.Configuration.AzureTableStorage;

namespace SFA.DAS.Recruit.Api;

public partial class Startup
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _hostingEnvironment;

    public Startup(IConfiguration configuration, IWebHostEnvironment env)
    {
        var config = new ConfigurationBuilder()
            .AddConfiguration(configuration)
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddEnvironmentVariables();
#if DEBUG
        config
            .AddJsonFile("appsettings.json", optional:true)
            .AddJsonFile("appsettings.Development.json", optional: true);   
#endif                
         
        config.AddAzureTableStorage(
            options => {
                options.ConfigurationKeys = configuration["ConfigNames"].Split(",");
                options.EnvironmentName = configuration["Environment"];
                options.StorageConnectionString = configuration["ConfigurationStorageConnectionString"];
                options.PreFixConfigurationKeys = false;
            }
        );

        _configuration = config.Build();
        _hostingEnvironment = env;
    }
}