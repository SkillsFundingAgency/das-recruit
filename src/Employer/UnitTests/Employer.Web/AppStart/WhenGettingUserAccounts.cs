using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using AutoFixture.NUnit3;
using Esfa.Recruit.Employer.Web.AppStart;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.AppStart;

public class WhenGettingUserAccounts
{
    [Test, MoqAutoData]
    public async Task Then_The_Accounts_Are_Returned(
        string userId, 
        string email,
        GetUserAccountsResponse getUserAccountsResponse,
        [Frozen] Mock<IRecruitVacancyClient> recruitVacancyClient,
        UserAccountService handler)
    {
        getUserAccountsResponse.IsSuspended = false;
        recruitVacancyClient.Setup(x => x.GetEmployerIdentifiersAsync(userId, email))
            .ReturnsAsync(getUserAccountsResponse);

        var actual = await handler.GetUserAccounts(userId, email);

        actual.EmployerAccounts.Should().BeEquivalentTo(getUserAccountsResponse.UserAccounts);
        actual.IsSuspended.Should().BeFalse();
        actual.FirstName.Should().Be(getUserAccountsResponse.FirstName);
        actual.LastName.Should().Be(getUserAccountsResponse.LastName);
    }
    
}