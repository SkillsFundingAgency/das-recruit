using System;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Configuration
{
    public static class RecruitEnvironment
    {
        public static string EnvironmentName => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        public static bool IsDevelopment => EnvironmentName?.Equals("Development", StringComparison.CurrentCultureIgnoreCase) ?? false;
    }
}
