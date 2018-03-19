using Esfa.Recruit.Employer.Web.Configuration;
using System.Security.Claims;

namespace Esfa.Recruit.Employer.Web.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetDisplayName(this ClaimsPrincipal user)
        {
            return user.FindFirstValue(EmployerRecruitClaims.IdamsUserDisplayNameClaimTypeIdentifier);
        }
    }
}
