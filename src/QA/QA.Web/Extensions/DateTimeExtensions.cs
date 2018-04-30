using System;

namespace Esfa.Recruit.Qa.Web.Extensions
{
    //NOTE: Duplicated from Employer solution
    public static class DateTimeExtensions
    {
        private const string DisplayDateFormat = "dd MMM yyyy";
        
        public static string AsDisplayDate(this DateTime date)
        {
            return date.ToString(DisplayDateFormat);
        }
    }
}