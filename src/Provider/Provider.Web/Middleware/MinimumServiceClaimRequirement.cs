using Esfa.Recruit.Provider.Web.Configuration;
using Microsoft.AspNetCore.Authorization;

namespace Esfa.Recruit.Provider.Web.Middleware
{
    public class MinimumServiceClaimRequirement : IAuthorizationRequirement
    {
        public ServiceClaim MinimumServiceClaim { get; set; }

        public MinimumServiceClaimRequirement(ServiceClaim minimumServiceClaim)
        {
            MinimumServiceClaim = minimumServiceClaim;
        }
    }
}
