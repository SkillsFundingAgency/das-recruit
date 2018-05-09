using System.Security.Claims;
using Esfa.Recruit.Qa.Web.Configuration;

namespace Esfa.Recruit.Qa.Web.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetUserId(this ClaimsPrincipal user)
        {
            return user.FindFirstValue(StaffIdamsClaims.IdamsUserIdClaimTypeIdentifier);
        }
    }
}