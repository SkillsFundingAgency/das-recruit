using System.Security.Claims;
using Esfa.Recruit.Qa.Web.Security;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Qa.Web.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static VacancyUser GetVacancyUser(this ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(StaffIdamsClaims.IdamsUserIdClaimTypeIdentifier);
            var name = user.FindFirstValue(StaffIdamsClaims.IdamsUserNameClaimTypeIdentifier);

            return new VacancyUser 
            {
                UserId  = userId,
                Name = name,
                Email = userId
            };
        }
    }
}