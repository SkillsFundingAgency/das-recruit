using System;
using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Provider.Web.Configuration;

namespace Esfa.Recruit.Provider.Web.Extensions
{
    public static class ServiceClaimExtensions
    {
        private static readonly List<string> ServiceClaimsList = Enum.GetNames(typeof(ServiceClaim)).ToList();

        public static bool IsServiceClaim(this string claim)
        {
            return ServiceClaimsList.Contains(claim);
        }
    }
}
