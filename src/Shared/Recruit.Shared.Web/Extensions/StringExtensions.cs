using System;
using System.Globalization;

namespace Esfa.Recruit.Shared.Web.Extensions
{

    public static class StringExtensions
    {
        private static readonly IFormatProvider _ukCulture = new CultureInfo("en-GB");
        private const int incodeLength = 3;
        
        public static string ToDateQueryString(this string value)
        {
            return value.Replace("/", string.Empty);
        }
        public static string AsWholeMoneyAmount(this string value)
        {
            return value.Replace(".00", "");
        }


        public static string AsPostcode(this string postcode)
        {
            if (postcode?.Length > incodeLength)
            {
                postcode = postcode.ToUpper().Replace(" ", "");
                postcode = postcode.Insert(postcode.Length - incodeLength, " ");
            }

            return postcode;
        }

        public static DateTime? AsDateTimeUk(this string date)
        {
            if(DateTime.TryParseExact(date, "d/M/yyyy", _ukCulture, DateTimeStyles.AssumeUniversal, out var d))
            {
                return d;
            }

            return null;
        }

        public static decimal? AsMoney(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }
            
            if (decimal.TryParse(text, out var d))
            {
                if (decimal.Round(d, 2, MidpointRounding.AwayFromZero) == d)
                {
                    return d;
                }
                return null;
            }

            return null;
        }

        public static decimal? AsDecimal(this string text, int decimals)
        {
            if (decimal.TryParse(text, out var d))
            {
                var roundedDecimal = decimal.Round(d, decimals, MidpointRounding.AwayFromZero);
                if (roundedDecimal == d)
                {
                    return roundedDecimal;
                }
                return null;
            }

            return null;
        }
    }
}
