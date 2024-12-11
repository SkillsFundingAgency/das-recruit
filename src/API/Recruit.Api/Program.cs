using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace SFA.DAS.Recruit.Api;

public class Program
{
    public static void Main(string[] args)
    {
        CreateWebHostBuilder(args).Build().Run();
    }

    private static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
        WebHost.CreateDefaultBuilder(args)
            .ConfigureKestrel(c => c.AddServerHeader = false)
            .UseKestrel()
            .UseUrls("https://localhost:5040")
            .UseStartup<Startup>();
}