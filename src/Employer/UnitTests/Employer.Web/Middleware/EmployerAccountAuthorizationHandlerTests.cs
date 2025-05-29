using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Employer.Web.Authorization;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Middleware;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.EmployerAccount;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.GovUK.Auth.Employer;
using SFA.DAS.Testing.AutoFixture;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Middleware;

public class EmployerAccountAuthorizationHandlerTests
{
    [Test]
    [MoqInlineAutoData(EmployerUserRole.Owner, true, "Owner")]
    [MoqInlineAutoData(EmployerUserRole.Transactor, true, "Owner")]
    [MoqInlineAutoData(EmployerUserRole.Viewer, true, "Owner")]
    [MoqInlineAutoData(EmployerUserRole.Owner, false, "Transactor")]
    [MoqInlineAutoData(EmployerUserRole.Transactor, true, "Transactor")]
    [MoqInlineAutoData(EmployerUserRole.Viewer, true, "Transactor")]
    [MoqInlineAutoData(EmployerUserRole.Owner, false, "Viewer")]
    [MoqInlineAutoData(EmployerUserRole.Transactor, false, "Viewer")]
    [MoqInlineAutoData(EmployerUserRole.Viewer, true, "Viewer")]
    [MoqInlineAutoData(EmployerUserRole.Viewer, false, "noRole")]
    public async Task Then_Returns_True_If_Employer_Is_Authorized_For_Owner_Role_And_Owner(
        EmployerUserRole minimumRole,
        bool accessResult,
        string roleOnClaims,
        EmployerUserAccountItem employerIdentifier,
        EmployerAccountOwnerOrTransactorRequirement ownerRequirement,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        EmployerAccountAuthorizationHandler authorizationHandler)
    {
        //Arrange
        employerIdentifier.Role = roleOnClaims;
        employerIdentifier.AccountId = employerIdentifier.AccountId.ToUpper();
        var employerAccounts = new Dictionary<string, EmployerUserAccountItem>{{employerIdentifier.AccountId, employerIdentifier}};
        var claim = new Claim(EmployerRecruitClaims.AccountsClaimsTypeIdentifier, JsonConvert.SerializeObject(employerAccounts));
        var claimsPrinciple = new ClaimsPrincipal(new[] {new ClaimsIdentity(new[] {claim})});
        var context = new AuthorizationHandlerContext(new [] {ownerRequirement}, claimsPrinciple, null);
        var httpContext = new DefaultHttpContext(new FeatureCollection());
        httpContext.Request.RouteValues.Add(RouteValueKeys.EmployerAccountHashedId,employerIdentifier.AccountId);
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
        
        //Act
        var actual = await authorizationHandler.IsEmployerAuthorized(context, minimumRole);

        //Assert
        actual.Should().Be(accessResult);
    }
    
    [Test, MoqAutoData]
    public async Task Then_Returns_False_If_Employer_Is_Not_Authorized(
        string accountId,
        EmployerUserAccountItem employerIdentifier,
        EmployerAccountOwnerOrTransactorRequirement ownerRequirement,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        EmployerAccountAuthorizationHandler authorizationHandler)
    {
        //Arrange
        employerIdentifier.Role = "Owner";
        employerIdentifier.AccountId = employerIdentifier.AccountId.ToUpper();
        var employerAccounts = new Dictionary<string, EmployerUserAccountItem>{{employerIdentifier.AccountId, employerIdentifier}};
        var claim = new Claim(EmployerRecruitClaims.AccountsClaimsTypeIdentifier, JsonConvert.SerializeObject(employerAccounts));
        var claimsPrinciple = new ClaimsPrincipal(new[] {new ClaimsIdentity(new[] {claim})});
        var context = new AuthorizationHandlerContext(new[] {ownerRequirement}, claimsPrinciple, null);
        var responseMock = new FeatureCollection();
        var httpContext = new DefaultHttpContext(responseMock);
        httpContext.Request.RouteValues.Add(RouteValueKeys.EmployerAccountHashedId,accountId.ToUpper());
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        //Act
        var actual = await authorizationHandler.IsEmployerAuthorized(context, EmployerUserRole.Owner);

        //Assert
        actual.Should().BeFalse();
    }

    [Test, MoqAutoData]
    public async Task Then_If_Not_In_Context_Claims_EmployerAccountService_Checked_And_True_Returned_If_Exists_For_GovSignIn(
        string accountId,
        string userId,
        string email,
        EmployerUserAccountItem employerIdentifier,
        EmployerAccountOwnerOrTransactorRequirement ownerRequirement,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        [Frozen] Mock<IGovAuthEmployerAccountService> employerAccountService,
        EmployerAccountAuthorizationHandler authorizationHandler)
    {
        //Arrange
        employerIdentifier.AccountId = accountId.ToUpper();
        employerIdentifier.Role = "Owner";
        employerAccountService.Setup(x => x.GetUserAccounts(userId, email))
            .ReturnsAsync(new EmployerUserAccounts
            {
                EmployerAccounts = new List<EmployerUserAccountItem>{ employerIdentifier }
            });
        
        var userClaim = new Claim(ClaimTypes.NameIdentifier, userId);
        var employerAccounts = new Dictionary<string, EmployerUserAccountItem>{{employerIdentifier.AccountId, employerIdentifier}};
        var employerAccountClaim = new Claim(EmployerRecruitClaims.AccountsClaimsTypeIdentifier, JsonConvert.SerializeObject(employerAccounts));
        var claimsPrinciple = new ClaimsPrincipal(new[] {new ClaimsIdentity(new[] {employerAccountClaim, userClaim, new Claim(ClaimTypes.Email, email)})});
        var context = new AuthorizationHandlerContext(new[] {ownerRequirement}, claimsPrinciple, null);
        var responseMock = new FeatureCollection();
        var httpContext = new DefaultHttpContext(responseMock);
        httpContext.Request.RouteValues.Add(RouteValueKeys.EmployerAccountHashedId,accountId.ToUpper());
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
        
        //Act
        var actual = await authorizationHandler.IsEmployerAuthorized(context, EmployerUserRole.Owner);

        //Assert
        actual.Should().BeTrue();
        
    }
    
    [Test, MoqAutoData]
    public async Task Then_If_Not_In_Context_Claims_EmployerAccountService_Checked_And_False_Returned_If_Not_Exists(
        string accountId,
        string userId,
        EmployerUserAccountItem employerIdentifier,
        EmployerAccountOwnerOrTransactorRequirement ownerRequirement,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        [Frozen] Mock<IGovAuthEmployerAccountService> employerAccountService,
        [Frozen] Mock<IConfiguration> configuration,
        EmployerAccountAuthorizationHandler authorizationHandler)
    {
        //Arrange
        employerIdentifier.AccountId = employerIdentifier.AccountId.ToUpper();
        employerIdentifier.Role = "Owner";
        employerAccountService.Setup(x => x.GetUserAccounts(userId,""))
            .ReturnsAsync(new EmployerUserAccounts()
            {
                EmployerAccounts = new List<EmployerUserAccountItem>{ employerIdentifier }
            });
        
        var userClaim = new Claim(EmployerRecruitClaims.IdamsUserIdClaimTypeIdentifier, userId);
        var employerAccounts = new Dictionary<string, EmployerUserAccountItem>{{employerIdentifier.AccountId, employerIdentifier}};
        var employerAccountClaim = new Claim(EmployerRecruitClaims.AccountsClaimsTypeIdentifier, JsonConvert.SerializeObject(employerAccounts));
        var claimsPrinciple = new ClaimsPrincipal(new[] {new ClaimsIdentity(new[] {employerAccountClaim, userClaim})});
        var context = new AuthorizationHandlerContext(new[] {ownerRequirement}, claimsPrinciple, null);
        var responseMock = new FeatureCollection();
        var httpContext = new DefaultHttpContext(responseMock);
        httpContext.Request.RouteValues.Add(RouteValueKeys.EmployerAccountHashedId,accountId.ToUpper());
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
        
        //Act
        var actual = await authorizationHandler.IsEmployerAuthorized(context, EmployerUserRole.Owner);

        //Assert
        actual.Should().BeFalse();
    }
    
    [Test, MoqAutoData]
    public async Task Then_Returns_False_If_AccountId_Not_In_Url(
        EmployerUserAccountItem employerIdentifier,
        EmployerAccountOwnerOrTransactorRequirement ownerRequirement,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        EmployerAccountAuthorizationHandler authorizationHandler)
    {
        //Arrange
        employerIdentifier.Role = "Owner";
        employerIdentifier.AccountId = employerIdentifier.AccountId.ToUpper();
        var employerAccounts = new Dictionary<string, EmployerUserAccountItem>{{employerIdentifier.AccountId, employerIdentifier}};
        var claim = new Claim(EmployerRecruitClaims.AccountsClaimsTypeIdentifier, JsonConvert.SerializeObject(employerAccounts));
        var claimsPrinciple = new ClaimsPrincipal(new[] {new ClaimsIdentity(new[] {claim})});
        var context = new AuthorizationHandlerContext(new[] {ownerRequirement}, claimsPrinciple, null);
        var responseMock = new FeatureCollection();
        var httpContext = new DefaultHttpContext(responseMock);
        httpContext.Request.RouteValues.Clear();
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
        
        //Act
        var actual = await authorizationHandler.IsEmployerAuthorized(context, EmployerUserRole.Viewer);

        //Assert
        actual.Should().BeFalse();
    }

    [Test, MoqAutoData]
    public async Task Then_Returns_False_If_No_Matching_AccountIdentifier_Claim_Found(
        EmployerUserAccountItem employerIdentifier,
        EmployerAccountOwnerOrTransactorRequirement ownerRequirement,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        EmployerAccountAuthorizationHandler authorizationHandler)
    {
        //Arrange
        employerIdentifier.Role = "Viewer-Owner-Transactor";
        employerIdentifier.AccountId = employerIdentifier.AccountId.ToUpper();
        var employerAccounts = new Dictionary<string, EmployerUserAccountItem>{{employerIdentifier.AccountId, employerIdentifier}};
        var claim = new Claim("SomeOtherClaim", JsonConvert.SerializeObject(employerAccounts));
        var claimsPrinciple = new ClaimsPrincipal(new[] {new ClaimsIdentity(new[] {claim})});
        var context = new AuthorizationHandlerContext(new[] {ownerRequirement}, claimsPrinciple, null);
        var responseMock = new FeatureCollection();
        var httpContext = new DefaultHttpContext(responseMock);
        httpContext.Request.RouteValues.Add(RouteValueKeys.EmployerAccountHashedId,employerIdentifier.AccountId);
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
        
        //Act
        var actual = await authorizationHandler.IsEmployerAuthorized(context, EmployerUserRole.Viewer);

        //Assert
        actual.Should().BeFalse();
    }
    
    [Test, MoqAutoData]
    public async Task Then_Returns_False_If_No_Matching_NameIdentifier_Claim_Found_For_GovSignIn(
        EmployerUserAccountItem employerIdentifier,
        EmployerAccountOwnerOrTransactorRequirement ownerRequirement,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        EmployerAccountAuthorizationHandler authorizationHandler)
    {
        //Arrange
        employerIdentifier.Role = "Viewer-Owner-Transactor";
        employerIdentifier.AccountId = employerIdentifier.AccountId.ToUpper();
        var employerAccounts = new Dictionary<string, EmployerUserAccountItem>{{employerIdentifier.AccountId, employerIdentifier}};
        var claim = new Claim("SomeOtherClaim", JsonConvert.SerializeObject(employerAccounts));
        var claimsPrinciple = new ClaimsPrincipal(new[] {new ClaimsIdentity(new[] {claim})});
        var context = new AuthorizationHandlerContext(new[] {ownerRequirement}, claimsPrinciple, null);
        var responseMock = new FeatureCollection();
        var httpContext = new DefaultHttpContext(responseMock);
        httpContext.Request.RouteValues.Add(RouteValueKeys.EmployerAccountHashedId,employerIdentifier.AccountId);
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
        
        //Act
        var actual = await authorizationHandler.IsEmployerAuthorized(context, EmployerUserRole.Viewer);

        //Assert
        actual.Should().BeFalse();
    }

    [Test, MoqAutoData]
    public async Task Then_Returns_False_If_The_Claim_Cannot_Be_Deserialized(
        EmployerUserAccountItem employerIdentifier,
        EmployerAccountOwnerOrTransactorRequirement ownerRequirement,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        EmployerAccountAuthorizationHandler authorizationHandler)
    {
        //Arrange
        employerIdentifier.Role = "Owner";
        employerIdentifier.AccountId = employerIdentifier.AccountId.ToUpper();
        var claim = new Claim(EmployerRecruitClaims.AccountsClaimsTypeIdentifier, JsonConvert.SerializeObject(employerIdentifier));
        var claimsPrinciple = new ClaimsPrincipal(new[] {new ClaimsIdentity(new[] {claim})});
        var context = new AuthorizationHandlerContext(new[] {ownerRequirement}, claimsPrinciple, null);
        var responseMock = new FeatureCollection();
        var httpContext = new DefaultHttpContext(responseMock);
        httpContext.Request.RouteValues.Add(RouteValueKeys.EmployerAccountHashedId,employerIdentifier.AccountId);
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
        
        //Act
        var actual = await authorizationHandler.IsEmployerAuthorized(context, EmployerUserRole.Owner);

        //Assert
        actual.Should().BeFalse();
    }
}