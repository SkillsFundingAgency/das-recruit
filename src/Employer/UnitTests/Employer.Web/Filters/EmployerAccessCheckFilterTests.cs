using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Controllers;
using Esfa.Recruit.Employer.Web.Exceptions;
using Esfa.Recruit.Employer.Web.Filters;
using Esfa.Recruit.Employer.Web.Services;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.EmployerAccount;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Moq;
using Xunit;
using DomainUser = Esfa.Recruit.Vacancies.Client.Domain.Entities.User;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Filters
{
    public class EmployerAccessCheckFilterTests
    {
        private EmployerAccessCheckFilter _sut;
        private Mock<ILevyDeclarationCookieWriter> _levyDeclarationCookieWriter;
        private Mock<IRecruitVacancyClient> _recruitVacancyClient;
        private Mock<IEmployerAccountProvider> _employerAccountProvider;
        private Mock<IEoiAgreementCookieWriter> _eoiAgreementCookieWriter;
        private Mock<IEmployerAccountTypeCookieWriter> _employerAccountTypeCookieWriter;
        private Mock<HttpContext> _httpContext;
        private Mock<ActionExecutionDelegate> _next;
        private ModelStateDictionary _modelState;
        private ControllerActionDescriptor _controllerActionDescriptor;
        private ActionExecutingContext _actionExecutingContext;
        private ActionContext _actionContext;
        private RouteData _routeData;
        private ClaimsPrincipal _user;
        private DomainUser _domainUser;
        private EmployerAccountDetails _account;
        private string _employerAccountTypeCookieValue;

        private const string LevyEmployerAccountTypeCookieValue = "USERID/EMPLOYERID/LEVY";
        private const string NonLevyEmployerAccountTypeCookieValue = "USERID/EMPLOYERID/NONLEVY";

        // Common
        [Theory]
        [InlineData(typeof(ErrorController))]
        [InlineData(typeof(LogoutController))]
        [InlineData(typeof(ExternalLinksController))]
        [InlineData(typeof(ContentPolicyReportController))]
        public async Task WhenRequestPageIsWhitelisted_ThenShouldCallNext(Type controllerType)
        {
            _controllerActionDescriptor.ControllerTypeInfo = controllerType.GetTypeInfo();

            await _sut.OnActionExecutionAsync(_actionExecutingContext, _next.Object);

            _next.Verify(x => x(), Times.Once);
        }

        [Theory]
        [InlineData(null, "Levy")]
        [InlineData("", "Levy")]
        [InlineData(null, "NonLevy")]
        [InlineData("", "NonLevy")]
        public async Task WhenUserHasNoEmployerAccountTypeCookie_ThenShouldWriteCookie(string cookieValue, string accountType)
        {
            _account.ApprenticeshipEmployerType = accountType;
            _employerAccountTypeCookieValue = cookieValue;

            try
            {
                await _sut.OnActionExecutionAsync(_actionExecutingContext, _next.Object);
            }
            catch (BlockedEmployerException)
            {
            }

            _next.Verify(x => x(), Times.Never);
            _employerAccountTypeCookieWriter.Verify(x =>
                x.WriteCookie(
                    It.IsAny<HttpResponse>(),
                    "USERID",
                    "EMPLOYERID",
                    _account.ApprenticeshipEmployerType)
             );
        }

        [Theory]
        [InlineData("DifferentUSERID/EMPLOYERID/Levy", "Levy")]
        [InlineData("USERID/DifferentEMPLOYERID/Levy", "Levy")]
        [InlineData("DifferentUSERID/EMPLOYERID/NonLevy", "NonLevy")]
        [InlineData("USERID/DifferentEMPLOYERID/NonLevy", "NonLevy")]
        public async Task WhenUserHasAnInvalidEmployerAccountTypeCookie_ThenShouldRewriteCookie(string cookieValue, string accountType)
        {
            _account.ApprenticeshipEmployerType = accountType;
            _employerAccountTypeCookieValue = cookieValue;

            try
            {
                await _sut.OnActionExecutionAsync(_actionExecutingContext, _next.Object);
            }
            catch (BlockedEmployerException)
            {
            }

            _next.Verify(x => x(), Times.Never);
            _employerAccountTypeCookieWriter.Verify(
                x => x.WriteCookie(It.IsAny<HttpResponse>(), "USERID", "EMPLOYERID", accountType));
        }

        [Fact]
        public async Task WhenLevyAccountAndAllElseFailsAndNotALevyPageRequested_ThenShouldRedirectToLevyDeclaration()
        {
            _employerAccountTypeCookieValue = LevyEmployerAccountTypeCookieValue;

            await _sut.OnActionExecutionAsync(_actionExecutingContext, _next.Object);

            _actionExecutingContext.Result.Should()
                .Match<RedirectToRouteResult>(x =>
                    x.RouteName == RouteNames.LevyDeclaration_Get
                    && (string)x.RouteValues["employerAccountId"] == "EMPLOYERID"
                );

            _next.Verify(x => x(), Times.Never);
        }

        [Fact]
        public async Task WhenNonLevyAccountAndAllElseFailsAndNotALevyPageRequested_ThenShouldThrowException()
        {
            _employerAccountTypeCookieValue = NonLevyEmployerAccountTypeCookieValue;

            await Assert.ThrowsAsync<BlockedEmployerException>(() =>
                 _sut.OnActionExecutionAsync(_actionExecutingContext, _next.Object));
            
            _next.Verify(x => x(), Times.Never);
        }


        [Theory]
        [InlineData(LevyEmployerAccountTypeCookieValue)]
        [InlineData(NonLevyEmployerAccountTypeCookieValue)]
        public async Task WhenAllElseFailsAndLevyPageRequested_ThenShouldCallNext(string cookieValue)
        {
            _employerAccountTypeCookieValue = cookieValue;
            _controllerActionDescriptor.ControllerTypeInfo = typeof(LevyDeclarationController).GetTypeInfo();

            await _sut.OnActionExecutionAsync(_actionExecutingContext, _next.Object);

            _next.Verify(x => x(), Times.Once);
        }

        [Fact]
        public async Task WhenInvalidEoiCookie_ThenShouldRewriteCookie()
        {
            _employerAccountTypeCookieValue = LevyEmployerAccountTypeCookieValue;

            _eoiAgreementCookieWriter
                .Setup(x => x.GetCookieFromRequest(It.IsAny<HttpContext>()))
                .Returns("This cookied value is incorrect");

            await _sut.OnActionExecutionAsync(_actionExecutingContext, _next.Object);

            _next.Verify(x => x(), Times.Never);
            _eoiAgreementCookieWriter.Verify(x => x.WriteCookie(It.IsAny<HttpResponse>(), "USERID", "EMPLOYERID", false), Times.Once);
        }

        [Fact]
        public async Task WhenValidEoiCookieWithTrueValue_ThenShouldCallNext()
        {
            _employerAccountTypeCookieValue = NonLevyEmployerAccountTypeCookieValue;

            _eoiAgreementCookieWriter
                .Setup(x => x.GetCookieFromRequest(It.IsAny<HttpContext>()))
                .Returns("USERID/EMPLOYERID/True");

            await _sut.OnActionExecutionAsync(_actionExecutingContext, _next.Object);

            _next.Verify(x => x(), Times.Once);
        }

        [Fact]
        public async Task WhenNoEoiCookieButHasEoi_ThenShouldWriteEoiCookie()
        {
            _employerAccountTypeCookieValue = NonLevyEmployerAccountTypeCookieValue;
            _account.AccountAgreementType = AccountAgreementType.NonLevyExpressionOfInterest;

            await _sut.OnActionExecutionAsync(_actionExecutingContext, _next.Object);

            _next.Verify(x => x(), Times.Never);
            _eoiAgreementCookieWriter.Verify(x => x.WriteCookie(It.IsAny<HttpResponse>(), "USERID", "EMPLOYERID", true));
        }

        [Fact]
        public async Task WhenNoEoiCookieButHasEoi_ThenShouldRedirectToDashboard()
        {
            _employerAccountTypeCookieValue = NonLevyEmployerAccountTypeCookieValue;
            _account.AccountAgreementType = AccountAgreementType.NonLevyExpressionOfInterest;

            await _sut.OnActionExecutionAsync(_actionExecutingContext, _next.Object);

            _next.Verify(x => x(), Times.Never);
            _actionExecutingContext.Result.Should()
                .Match<RedirectToRouteResult>(x =>
                    x.RouteName == RouteNames.Dashboard_Get
                    && (string)x.RouteValues["employerAccountId"] == "EMPLOYERID"
                );
        }

        // Levy tests
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task WhenLevyEmployerWithAnInvalidLevyCookie_ThenShouldRewriteCookie(bool permitAccessToEmployer)
        {
            _employerAccountTypeCookieValue = LevyEmployerAccountTypeCookieValue;
            if (permitAccessToEmployer)
                _domainUser.AccountsDeclaredAsLevyPayers.Add("EMPLOYERID");

            _levyDeclarationCookieWriter
                .Setup(x => x.GetCookieFromRequest(It.IsAny<HttpContext>()))
                .Returns("This is not a valid cookie");

            await _sut.OnActionExecutionAsync(_actionExecutingContext, _next.Object);

            if (permitAccessToEmployer)
                _next.Verify(x => x(), Times.Once);
            else
                _next.Verify(x => x(), Times.Never);

            _levyDeclarationCookieWriter.Verify(
                x => x.WriteCookie(It.IsAny<HttpResponse>(), "USERID", "EMPLOYERID", permitAccessToEmployer));
        }

        [Fact]
        public async Task WhenLevyEmployerWithLevyCookieAndLevyPageRequested_ThenShouldRedirectToDashboard()
        {
            _employerAccountTypeCookieValue = LevyEmployerAccountTypeCookieValue;

            _levyDeclarationCookieWriter
                .Setup(x => x.GetCookieFromRequest(It.IsAny<HttpContext>()))
                .Returns("USERID/EMPLOYERID/True");
            _controllerActionDescriptor.ControllerTypeInfo = typeof(LevyDeclarationController).GetTypeInfo();

            await _sut.OnActionExecutionAsync(_actionExecutingContext, _next.Object);

            _next.Verify(x => x(), Times.Never);
            _actionExecutingContext.Result.Should()
                .Match<RedirectToRouteResult>(x =>
                    x.RouteName == RouteNames.Dashboard_Get
                    && (string)x.RouteValues["employerAccountId"] == "EMPLOYERID"
                );
        }

        [Fact]
        public async Task WhenLevyEmployerWithLevyCookieAndNotALevyPageRequested_ThenShouldCallNext()
        {
            _employerAccountTypeCookieValue = LevyEmployerAccountTypeCookieValue;
            _levyDeclarationCookieWriter
                .Setup(x => x.GetCookieFromRequest(It.IsAny<HttpContext>()))
                .Returns("USERID/EMPLOYERID/True");
            _controllerActionDescriptor.ControllerTypeInfo = typeof(VacanciesController).GetTypeInfo();

            await _sut.OnActionExecutionAsync(_actionExecutingContext, _next.Object);

            _next.Verify(x => x(), Times.Once);
        }

        [Fact]
        public async Task WhenLevyEmployerWithoutLevyCookieHasStoredDeclarationAndLevyPageRequested_ThenShouldWriteCookie()
        {
            _employerAccountTypeCookieValue = LevyEmployerAccountTypeCookieValue;
            _domainUser.AccountsDeclaredAsLevyPayers.Add("EMPLOYERID");
            _controllerActionDescriptor.ControllerTypeInfo = typeof(LevyDeclarationController).GetTypeInfo();

            await _sut.OnActionExecutionAsync(_actionExecutingContext, _next.Object);

            _levyDeclarationCookieWriter
                .Verify(x => x.WriteCookie(It.IsAny<HttpResponse>(), "USERID", "EMPLOYERID", true));
        }

        [Fact]
        public async Task WhenLevyEmployerWithoutLevyCookieHasStoredDeclarationAndLevyPageRequested_ThenShouldRedirectToDashboard()
        {
            _employerAccountTypeCookieValue = LevyEmployerAccountTypeCookieValue;
            _domainUser.AccountsDeclaredAsLevyPayers.Add("EMPLOYERID");
            _controllerActionDescriptor.ControllerTypeInfo = typeof(LevyDeclarationController).GetTypeInfo();

            await _sut.OnActionExecutionAsync(_actionExecutingContext, _next.Object);

            _next.Verify(x => x(), Times.Never);
            _actionExecutingContext.Result.Should()
                .Match<RedirectToRouteResult>(x =>
                    x.RouteName == RouteNames.Dashboard_Get
                    && (string)x.RouteValues["employerAccountId"] == "EMPLOYERID"
                );
        }

        [Fact]
        public async Task WhenLevyEmployerWithoutLevyCookieHasStoredDeclarationAndNotALevyPageRequested_ThenShouldCallNext()
        {
            _employerAccountTypeCookieValue = LevyEmployerAccountTypeCookieValue;
            _domainUser.AccountsDeclaredAsLevyPayers.Add("EMPLOYERID");

            await _sut.OnActionExecutionAsync(_actionExecutingContext, _next.Object);

            _next.Verify(x => x(), Times.Once);
        }

        public EmployerAccessCheckFilterTests()
        {
            _modelState = new ModelStateDictionary();
            _routeData = new RouteData();
            _routeData.Values[RouteValues.EmployerAccountId] = "EMPLOYERID";

            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(EmployerRecruitClaims.IdamsUserIdClaimTypeIdentifier, "USERID"));
            _user = new ClaimsPrincipal(identity);

            _httpContext = new Mock<HttpContext>();
            _httpContext.SetupGet(x => x.User).Returns(_user);

            _controllerActionDescriptor = new ControllerActionDescriptor();
            _controllerActionDescriptor.ControllerTypeInfo = typeof(VacanciesController).GetTypeInfo();

            _actionContext = new ActionContext(
                _httpContext.Object,
                _routeData,
                _controllerActionDescriptor,
                _modelState
            );

            _actionExecutingContext = new ActionExecutingContext(
                _actionContext,
                new List<IFilterMetadata>(),
                new Dictionary<string, object>(),
                _controllerActionDescriptor
            );

            _levyDeclarationCookieWriter = new Mock<ILevyDeclarationCookieWriter>();
            _recruitVacancyClient = new Mock<IRecruitVacancyClient>();
            _employerAccountProvider = new Mock<IEmployerAccountProvider>();
            _eoiAgreementCookieWriter = new Mock<IEoiAgreementCookieWriter>();

            _employerAccountTypeCookieWriter = new Mock<IEmployerAccountTypeCookieWriter>();
            _employerAccountTypeCookieWriter
                .Setup(x => x.GetCookieFromRequest(It.IsAny<HttpContext>()))
                .Returns(() => _employerAccountTypeCookieValue);

            _next = new Mock<ActionExecutionDelegate>();

            _domainUser = new DomainUser();
            _recruitVacancyClient
                .Setup(x => x.GetUsersDetailsAsync("USERID"))
                .ReturnsAsync(_domainUser);

            _account = new EmployerAccountDetails
            {
                AccountAgreementType = AccountAgreementType.Inconsistent
            };
            _employerAccountProvider
                    .Setup(x => x.GetEmployerAccountDetailsAsync("EMPLOYERID"))
                    .ReturnsAsync(_account);

            _sut = new EmployerAccessCheckFilter(_levyDeclarationCookieWriter.Object,
                _recruitVacancyClient.Object,
                _employerAccountProvider.Object,
                _eoiAgreementCookieWriter.Object,
                _employerAccountTypeCookieWriter.Object);
        }

    }
}
