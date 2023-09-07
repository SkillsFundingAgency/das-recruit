using Esfa.Recruit.Provider.Web.Configuration;
using System.Security.Claims;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Provider.Web.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetDisplayName(this ClaimsPrincipal user)
        {
            return user.FindFirstValue(ProviderRecruitClaims.IdamsUserDisplayNameClaimTypeIdentifier) 
                   ?? user.FindFirstValue(ProviderRecruitClaims.DfEUserDisplayNameClaimTypeIdentifier);
        }

        public static string GetEmailAddress(this ClaimsPrincipal user)
        {
            return user.FindFirstValue(ProviderRecruitClaims.IdamsUserEmailClaimTypeIdentifier) 
                   ?? user.FindFirstValue("email");
        }

        public static string GetUserName(this ClaimsPrincipal user)
        {
            return user.FindFirstValue(ProviderRecruitClaims.IdamsUserNameClaimTypeIdentifier)
                   ?? user.FindFirstValue(ProviderRecruitClaims.DfEUserNameClaimTypeIdentifier);
        }

        public static long GetUkprn(this ClaimsPrincipal user)
        {
            var ukprn = user.FindFirstValue(ProviderRecruitClaims.IdamsUserUkprnClaimsTypeIdentifier) 
                        ?? user.FindFirstValue(ProviderRecruitClaims.DfEUkprnClaimsTypeIdentifier) ;

            return long.Parse(ukprn);
        }

        public static VacancyUser ToVacancyUser(this ClaimsPrincipal user)
        {
            return new VacancyUser
            {
                UserId = user.GetUserName(),
                Name = user.GetDisplayName(),
                Email = user.GetEmailAddress(),
                Ukprn = user.GetUkprn()
            };
        }
    }
}