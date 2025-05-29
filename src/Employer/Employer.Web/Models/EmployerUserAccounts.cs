using System;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using System.Collections.Generic;
using System.Linq;
using SFA.DAS.GovUK.Auth.Employer;

namespace Esfa.Recruit.Employer.Web.Models
{
    public class EmployerUserAccounts
    {
        public string Email { get; set; }
        public string EmployerUserId { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public IEnumerable<EmployerUserAccountItem> EmployerAccounts { get; set; }

        public static implicit operator EmployerUserAccounts(GetUserAccountsResponse source)
        {
            if (source?.UserAccounts == null)
            {
                return new EmployerUserAccounts
                {
                    FirstName = source?.FirstName,
                    LastName = source?.LastName,
                    EmployerUserId = source?.EmployerUserId,
                    EmployerAccounts = new List<EmployerUserAccountItem>(),
                };
            }

            return new EmployerUserAccounts
            {
                FirstName = source.FirstName,
                LastName = source.LastName,
                EmployerUserId = source.EmployerUserId,
                EmployerAccounts = source.UserAccounts != null ? source.UserAccounts.Select(c => new EmployerUserAccountItem
                {
                    Role = c.Role,
                    AccountId = c.AccountId,
                    EmployerName = c.EmployerName,
                    ApprenticeshipEmployerType = Enum.Parse<ApprenticeshipEmployerType>(c.ApprenticeshipEmployerType)
                }).ToList() : [],
            };
        }
    }
}