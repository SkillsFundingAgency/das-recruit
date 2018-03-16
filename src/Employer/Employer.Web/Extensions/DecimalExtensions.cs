using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Extensions
{
    public static class DecimalExtensions
    {
        public static string AsMoney(this decimal dec)
        {
            return $"{dec:N2}".Replace(".00", "");
        }
    }
}
