using System;

namespace Esfa.Recruit.Employer.Web.Extensions
{
    public static class DateTimeExtensions
    {
        public static string ToMonthYearString(this DateTime date)
        {
            return date.Year == DateTime.MinValue.Year ? "Current" : date.ToString("MMM yyyy");
        }
    }
}
