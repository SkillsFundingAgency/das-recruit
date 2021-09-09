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
            Configuration = new ConfigurationBuilder()
                .AddConfiguration(configuration)
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddAzureTableStorage(
                    options => {
                        options.ConfigurationKeys = configuration["ConfigNames"].Split(",");
                        options.EnvironmentName = configuration["Environment"];
                        options.StorageConnectionString = configuration["ConfigurationStorageConnectionString"];
                        options.PreFixConfigurationKeys = false;
                    }
                )
                .Build();

            HostingEnvironment = env;
        }
    }
}
