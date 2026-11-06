using Esfa.Recruit.Provider.Web.Configuration;
using Microsoft.AspNetCore.Authorization;

namespace Esfa.Recruit.Provider.Web.Middleware;

public class MinimumServiceClaimRequirement(ServiceClaim minimumServiceClaim) : IAuthorizationRequirement
{
    public ServiceClaim MinimumServiceClaim { get; set; } = minimumServiceClaim;
}