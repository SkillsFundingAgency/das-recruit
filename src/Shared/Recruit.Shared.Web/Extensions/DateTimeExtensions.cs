using System;

namespace Esfa.Recruit.Shared.Web.Extensions
{
    public static class DateTimeExtensions
    {
        private const string DisplayDateFormat = "dd MMM yyyy";
        
        public static string AsDisplayDate(this DateTime date)
        {
            return date.ToString(DisplayDateFormat);
        }
    }
}
