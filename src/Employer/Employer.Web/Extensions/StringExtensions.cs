﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Extensions
{

    public static class StringExtensions
    {
        private static readonly IFormatProvider _ukCulture = new CultureInfo("en-GB");
        const int incodeLength = 3;
        
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
