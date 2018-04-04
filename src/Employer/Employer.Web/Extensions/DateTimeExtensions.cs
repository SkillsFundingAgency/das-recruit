using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Extensions
{

    public static class DateTimeExtensions
    {
        private const string DisplayDateFormat = "dd MMM yyyy";
        const int incodeLength = 3;
        
        public static string AsDisplayDate(this DateTime date)
        {
            return date.ToString(DisplayDateFormat);
        }
    }
}
