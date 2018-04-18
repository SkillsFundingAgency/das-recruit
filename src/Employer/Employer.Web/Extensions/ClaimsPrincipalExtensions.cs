﻿using Esfa.Recruit.Employer.Web.Configuration;
using System.Security.Claims;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Employer.Web.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetDisplayName(this ClaimsPrincipal user)
        {
            return user.FindFirstValue(EmployerRecruitClaims.IdamsUserDisplayNameClaimTypeIdentifier);
        }

        public static string GetEmailAddress(this ClaimsPrincipal user)
        {
            return user.FindFirstValue(EmployerRecruitClaims.IdamsUserEmailClaimTypeIdentifier);
        }

        public static string GetUserId(this ClaimsPrincipal user)
        {
            return user.FindFirstValue(EmployerRecruitClaims.IdamsUserIdClaimTypeIdentifier);
        }

        public static VacancyUser ToVacancyUser(this ClaimsPrincipal user)
        {
            return new VacancyUser
            {
                UserId = user.GetUserId(),
                Name = user.GetDisplayName(),
                Email = user.GetEmailAddress()
            };
        }
    }
}
