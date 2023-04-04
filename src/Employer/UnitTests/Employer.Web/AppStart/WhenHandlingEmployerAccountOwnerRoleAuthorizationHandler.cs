using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Employer.Web.Interfaces;
using Esfa.Recruit.Employer.Web.Middleware;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Moq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.AppStart
{
    public class WhenHandlingEmployerAccountOwnerRoleAuthorizationHandler
    {
        [Test, MoqAutoData]
        public async Task Then_Returns_Succeeded_If_Employer_Is_Authorized_For_Any_Role(
            EmployerAccountOwnerRequirement requirement,
            [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
            [Frozen] Mock<IEmployerAccountAuthorizationHandler> handler,
            EmployerAccountOwnerAuthorizationHandler authorizationHandler)
        {
            //Arrange
            var context = new AuthorizationHandlerContext(new[] { requirement }, new ClaimsPrincipal(), null);
            var httpContext = new DefaultHttpContext(new FeatureCollection());
            httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
            handler.Setup(x => x.IsEmployerAuthorized(context, false)).ReturnsAsync(true);

            //Act
            await authorizationHandler.HandleAsync(context);

            //Assert
            context.HasSucceeded.Should().BeTrue();
        }
        [Test, MoqAutoData]
        public async Task Then_Returns_Failed_If_Employer_Is_Not_Authorized_For_Any_Role(
            EmployerAccountOwnerRequirement requirement,
            [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
            [Frozen] Mock<IEmployerAccountAuthorizationHandler> handler,
            EmployerAccountOwnerAuthorizationHandler authorizationHandler)
        {
            //Arrange
            var context = new AuthorizationHandlerContext(new[] { requirement }, new ClaimsPrincipal(), null);
            var httpContext = new DefaultHttpContext(new FeatureCollection());
            httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
            handler.Setup(x => x.IsEmployerAuthorized(context, true)).ReturnsAsync(false);

            //Act
            await authorizationHandler.HandleAsync(context);

            //Assert
            context.HasSucceeded.Should().BeFalse();
        }
    }
}
