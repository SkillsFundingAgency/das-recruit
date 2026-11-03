using Esfa.Recruit.Provider.Web.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Esfa.Recruit.Provider.Web;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        using var loggerFactory = LoggerFactory.Create(_ => { });

        var startup = new Startup(
            builder.Configuration,
            builder.Environment,
            loggerFactory);

        startup.ConfigureServices(builder.Services);

        var app = builder.Build();

        startup.Configure(
            app,
            app.Environment,
            app.Services.GetRequiredService<IOptions<ExternalLinksConfiguration>>(),
            app.Configuration);
        app.Run();
    }
}