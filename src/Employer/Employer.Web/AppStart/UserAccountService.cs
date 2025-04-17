using System;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using SFA.DAS.GovUK.Auth.Employer;
using EmployerUserAccountItem = SFA.DAS.GovUK.Auth.Employer.EmployerUserAccountItem;
using EmployerUserAccounts = SFA.DAS.GovUK.Auth.Employer.EmployerUserAccounts;

namespace Esfa.Recruit.Employer.Web.AppStart;

public class UserAccountService : IGovAuthEmployerAccountService
{
    private readonly IRecruitVacancyClient _vacancyClient;

    public UserAccountService(IRecruitVacancyClient vacancyClient)
    {
        _vacancyClient = vacancyClient;
    }
    
    public async Task<EmployerUserAccounts> GetUserAccounts(string userId, string email)
    {
        var result = await _vacancyClient.GetEmployerIdentifiersAsync(userId, email);

        return new EmployerUserAccounts
        {
            EmployerAccounts = result.UserAccounts != null? result.UserAccounts.Select(c => new EmployerUserAccountItem
            {
                Role = c.Role,
                AccountId = c.AccountId,
                ApprenticeshipEmployerType = Enum.Parse<ApprenticeshipEmployerType>(c.ApprenticeshipEmployerType.ToString()),
                EmployerName = c.EmployerName,
            }).ToList() : [],
            FirstName = result.FirstName,
            IsSuspended = result.IsSuspended,
            LastName = result.LastName,
            EmployerUserId = result.EmployerUserId,
        };
    }
}