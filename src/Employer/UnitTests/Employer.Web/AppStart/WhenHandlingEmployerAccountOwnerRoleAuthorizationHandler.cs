using System.Security.Claims;
using AutoFixture.NUnit3;
using Esfa.Recruit.Employer.Web.Interfaces;
using Esfa.Recruit.Employer.Web.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.AppStart
{
    public class WhenHandlingEmployerAccountOwnerRoleAuthorizationHandler
    {
        [Test, MoqAutoData]
        public async Task Then_Returns_Succeeded_If_Employer_Is_Authorized_For_Any_Role(
            EmployerAccountOwnerOrTransactorRequirement orTransactorRequirement,
            [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
            [Frozen] Mock<IEmployerAccountAuthorizationHandler> handler,
            EmployerAccountOwnerOrTransactorAuthorizationHandler orTransactorAuthorizationHandler)
        {
            //Arrange
            var context = new AuthorizationHandlerContext(new[] { orTransactorRequirement }, new ClaimsPrincipal(), null);
            var httpContext = new DefaultHttpContext(new FeatureCollection());
            httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
            handler.Setup(x => x.IsEmployerAuthorized(context, EmployerUserRole.Transactor)).ReturnsAsync(true);

            //Act
            await orTransactorAuthorizationHandler.HandleAsync(context);

            //Assert
            context.HasSucceeded.Should().BeTrue();
        }
        [Test, MoqAutoData]
        public async Task Then_Returns_Failed_If_Employer_Is_Not_Authorized_For_Any_Role(
            EmployerAccountOwnerOrTransactorRequirement orTransactorRequirement,
            [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
            [Frozen] Mock<IEmployerAccountAuthorizationHandler> handler,
            EmployerAccountOwnerOrTransactorAuthorizationHandler orTransactorAuthorizationHandler)
        {
            //Arrange
            var context = new AuthorizationHandlerContext(new[] { orTransactorRequirement }, new ClaimsPrincipal(), null);
            var httpContext = new DefaultHttpContext(new FeatureCollection());
            httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
            handler.Setup(x => x.IsEmployerAuthorized(context, EmployerUserRole.Transactor)).ReturnsAsync(false);

            //Act
            await orTransactorAuthorizationHandler.HandleAsync(context);

            //Assert
            context.HasSucceeded.Should().BeFalse();
        }
    }
}
