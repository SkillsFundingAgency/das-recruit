using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Extensions
{
    public static class StringExtensions
    {
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
    }
}
