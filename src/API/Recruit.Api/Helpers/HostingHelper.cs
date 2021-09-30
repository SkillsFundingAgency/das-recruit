using System;

namespace SFA.DAS.Recruit.Api.Helpers
{
    public static class HostingHelper
    {
        public static string GetWebsiteInstanceId()
        {
            var id = Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID")?.Substring(0, 10) ?? "local";
            return id;
        }
    }
}