﻿using Esfa.Recruit.Vacancies.Client.Ioc;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Esfa.Recruit.Employer.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        private static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .ConfigureLogging(b => b.ConfigureRecruitLogging());
    }
}