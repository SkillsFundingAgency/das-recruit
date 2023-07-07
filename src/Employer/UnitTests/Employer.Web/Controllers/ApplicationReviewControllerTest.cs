using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Controllers;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.ViewModels.ApplicationReview;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Controllers
{
    [TestFixture]
    public class ApplicationReviewControllerTests
    {
        private ApplicationReviewController _controller;
        private Mock<IApplicationReviewOrchestrator> _orchestratorMock;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _orchestratorMock = new Mock<IApplicationReviewOrchestrator>();
            _controller = new ApplicationReviewController(_orchestratorMock.Object)
            {
                TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>())
            };

            var userClaims = new List<Claim>
            {
                new Claim(EmployerRecruitClaims.IdamsUserIdClaimTypeIdentifier,"userid"),
                new Claim(EmployerRecruitClaims.IdamsUserDisplayNameClaimTypeIdentifier,"displayname"),
                new Claim(EmployerRecruitClaims.IdamsUserEmailClaimTypeIdentifier,"email")
            };
            var userIdentity = new ClaimsIdentity(userClaims);
            var user = new ClaimsPrincipal(userIdentity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }


        [Test]
        public async Task ApplicationReview_WithValidModelStateAndVacancySharedByProvider_RedirectsToVacancyManage()
        {
            var editModel = _fixture.Create<ApplicationReviewEditModel>();
            editModel.Outcome = ApplicationReviewStatus.EmployerInterviewing;
            var vacancySharedByProvider = true;
            var candidateInfo = new ApplicationReviewCandidateInfo { FriendlyId = "123456", Name = "Username" };

            _orchestratorMock.Setup(o => o.GetApplicationReviewViewModelAsync(editModel))
                .ReturnsAsync(new ApplicationReviewViewModel());
            _orchestratorMock.Setup(o => o.PostApplicationReviewEditModelAsync(editModel, It.IsAny<VacancyUser>(), vacancySharedByProvider))
                .ReturnsAsync(candidateInfo);

            var result = await _controller.ApplicationReview(editModel, vacancySharedByProvider);

            Assert.IsInstanceOf<RedirectToRouteResult>(result);
            var redirectToRouteResult = (RedirectToRouteResult)result;
            Assert.AreEqual(RouteNames.VacancyManage_Get, redirectToRouteResult.RouteName);
            Assert.AreEqual(editModel.EmployerAccountId, redirectToRouteResult.RouteValues["EmployerAccountId"]);
            Assert.AreEqual(editModel.VacancyId, redirectToRouteResult.RouteValues["VacancyId"]);
            Assert.AreEqual(vacancySharedByProvider, redirectToRouteResult.RouteValues["vacancySharedByProvider"]);
            Assert.IsTrue(_controller.TempData.ContainsKey(TempDataKeys.ApplicationReviewStatusInfoMessage));
            var message = _controller.TempData[TempDataKeys.ApplicationReviewStatusInfoMessage];
            Assert.AreEqual(string.Format(InfoMessages.ApplicationEmployerReviewStatusHeader, candidateInfo.FriendlyId, candidateInfo.Name), message);
        }

        [Test]
        public async Task ApplicationReview_WithValidModelStateAndVacancyNotSharedByProvider_RedirectsToApplicationReviewConfirmation()
        {
            string TempDataARModel = "ApplicationReviewEditModel";
            var editModel = _fixture.Create<ApplicationReviewEditModel>();
            var vacancySharedByProvider = false;

            _orchestratorMock.Setup(o => o.GetApplicationReviewViewModelAsync(editModel))
                .ReturnsAsync(new ApplicationReviewViewModel());
            _controller.TempData[TempDataARModel] = JsonConvert.SerializeObject(editModel);

            var result = await _controller.ApplicationReview(editModel, vacancySharedByProvider);

            Assert.IsInstanceOf<RedirectToRouteResult>(result);
            var redirectToRouteResult = (RedirectToRouteResult)result;
            Assert.AreEqual(RouteNames.ApplicationReviewConfirmation_Get, redirectToRouteResult.RouteName);
            Assert.AreEqual(editModel.VacancyId, redirectToRouteResult.RouteValues["VacancyId"]);
            Assert.AreEqual(editModel.EmployerAccountId, redirectToRouteResult.RouteValues["EmployerAccountId"]);
            Assert.AreEqual(editModel.ApplicationReviewId, redirectToRouteResult.RouteValues["ApplicationReviewId"]);
        }
    }
}
