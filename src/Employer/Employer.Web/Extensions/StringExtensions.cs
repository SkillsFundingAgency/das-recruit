using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Extensions
{
    public static class StringExtensions
    {
        public static string AsPostcode(this string postcode)
        {
            if (postcode?.Length > 3)
            {
                postcode = postcode.Trim().ToUpper().Replace(" ", "");
                postcode = postcode.Insert(postcode.Length - 3, " ");
            }

            return postcode;
        }
    }
}
