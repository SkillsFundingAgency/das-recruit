﻿using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Employer.Web.Configuration;
using System.Security.Claims;
using Esfa.Recruit.Employer.Web.Models;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using Newtonsoft.Json;

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
            return user.FindFirstValue(EmployerRecruitClaims.IdamsUserEmailClaimTypeIdentifier) ?? user.FindFirstValue(ClaimTypes.Email);
        }

        public static string GetUserId(this ClaimsPrincipal user)
        {
            return user.FindFirstValue(EmployerRecruitClaims.IdamsUserIdClaimTypeIdentifier);
        }

        public static IEnumerable<string> GetEmployerAccounts(this ClaimsPrincipal user)
        {
            var employerAccountClaim = user.FindFirst(c => c.Type.Equals(EmployerRecruitClaims.AccountsClaimsTypeIdentifier));
            
            if (string.IsNullOrEmpty(employerAccountClaim?.Value))
                return Enumerable.Empty<string>();
            
            var employerAccounts = JsonConvert.DeserializeObject<Dictionary<string, EmployerIdentifier>>(employerAccountClaim.Value).Keys.Select(c=>c).ToList();
            return employerAccounts;
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
