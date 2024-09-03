using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Employer.Web.AppStart;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.AppStart;

public class WhenGettingClaims
{
    [Test, MoqAutoData]
    public async Task Then_The_Claims_Are_Used_To_Return_Account_Info(
        string userId, 
        string email,
        GetUserAccountsResponse getUserAccountsResponse,
        [Frozen] Mock<IRecruitVacancyClient> recruitVacancyClient,
        EmployerAccountPostAuthenticationClaimsHandler handler)
    {
        getUserAccountsResponse.IsSuspended = false;
        var context = ArrangeTokenValidatedContext(userId, email);
        recruitVacancyClient.Setup(x => x.GetEmployerIdentifiersAsync(userId, email))
            .ReturnsAsync(getUserAccountsResponse);

        var actual = await handler.GetClaims(context);

        actual.FirstOrDefault(c => c.Type.Equals(EmployerRecruitClaims.AccountsClaimsTypeIdentifier)).Value
            .Should().Be(
                JsonConvert.SerializeObject(getUserAccountsResponse.UserAccounts.ToDictionary(c => c.AccountId)));

        actual.FirstOrDefault(c => c.Type.Equals(EmployerRecruitClaims.IdamsUserIdClaimTypeIdentifier)).Value
            .Should().Be(getUserAccountsResponse.EmployerUserId);
        actual.FirstOrDefault(c => c.Type.Equals(ClaimTypes.AuthorizationDecision))?.Value?.Should().BeNullOrEmpty();
    }
    [Test, MoqAutoData]
    public async Task Then_The_Suspended_Claim_Is_Set_If_True(
        string userId, 
        string email,
        GetUserAccountsResponse getUserAccountsResponse,
        [Frozen] Mock<IRecruitVacancyClient> recruitVacancyClient,
        EmployerAccountPostAuthenticationClaimsHandler handler)
    {
        getUserAccountsResponse.IsSuspended = true;
        var context = ArrangeTokenValidatedContext(userId, email);
        recruitVacancyClient.Setup(x => x.GetEmployerIdentifiersAsync(userId, email))
            .ReturnsAsync(getUserAccountsResponse);

        var actual = await handler.GetClaims(context);

        actual.FirstOrDefault(c => c.Type.Equals(ClaimTypes.AuthorizationDecision)).Value
            .Should().Be("Suspended");
    }
    
    private TokenValidatedContext ArrangeTokenValidatedContext(string nameIdentifier, string emailAddress)
    {
        var identity = new ClaimsIdentity(new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, nameIdentifier),
            new Claim(ClaimTypes.Email, emailAddress)
        });
        
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(identity));
        return new TokenValidatedContext(new DefaultHttpContext(), new AuthenticationScheme(",","", typeof(TestAuthHandler)),
            new OpenIdConnectOptions(), Mock.Of<ClaimsPrincipal>(), new AuthenticationProperties())
        {
            Principal = claimsPrincipal
        };
    }
    
    private class TestAuthHandler : IAuthenticationHandler
    {
        public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
        {
            throw new NotImplementedException();
        }

        public Task<AuthenticateResult> AuthenticateAsync()
        {
            throw new NotImplementedException();
        }

        public Task ChallengeAsync(AuthenticationProperties? properties)
        {
            throw new NotImplementedException();
        }

        public Task ForbidAsync(AuthenticationProperties? properties)
        {
            throw new NotImplementedException();
        }
    }
}