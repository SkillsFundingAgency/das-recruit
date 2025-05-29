using Esfa.Recruit.Vacancies.Client.Ioc;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Esfa.Recruit.Qa.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        private static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseUrls("https://localhost:5025")
                .ConfigureLogging(b => b.ConfigureRecruitLogging());
        }
    }
}