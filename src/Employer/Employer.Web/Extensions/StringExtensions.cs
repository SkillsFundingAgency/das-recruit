using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Extensions
{
    public static class StringExtensions
    {
        public static string AsPostCode(this string postcode)
        {
            if (!string.IsNullOrEmpty(postcode) && postcode.Length > 3)
            {
                postcode = postcode.ToUpper().Replace(" ", "");
                var chars = postcode.ToCharArray().ToList();
                chars.Insert(chars.Count - 3, ' ');
                postcode = new string(chars.ToArray());
            }

            return postcode;
        }
    }
}
