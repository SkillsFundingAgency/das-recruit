using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using SFA.DAS.Configuration.AzureTableStorage;

namespace SFA.DAS.Recruit.Api
{
	public partial class Startup
    {
        public IConfiguration Configuration { get; }
        public IWebHostEnvironment HostingEnvironment { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            var config = new ConfigurationBuilder()
                .AddConfiguration(configuration)
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables();
                
            config
                .AddJsonFile("appsettings.json", optional:true)
                .AddJsonFile("appsettings.Development.json", optional: true);
            
            config.AddAzureTableStorage(
                    options => {
                        options.ConfigurationKeys = configuration["ConfigNames"].Split(",");
                        options.EnvironmentName = configuration["Environment"];
                        options.StorageConnectionString = configuration["ConfigurationStorageConnectionString"];
                        options.PreFixConfigurationKeys = false;
                    }
                );

            Configuration = config.Build();
            
            HostingEnvironment = env;
        }
    }
}
