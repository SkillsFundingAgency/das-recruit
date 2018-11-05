using System;

namespace Employer.Web.Configuration
{
    public static class Times
    {
        public static DateTime NextDay => DateTime.Today.ToUniversalTime().AddDays(1);
    }
}