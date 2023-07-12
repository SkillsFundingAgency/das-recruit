using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Controllers;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using Esfa.Recruit.Provider.Web.ViewModels.ApplicationReview;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Shared.Web.ViewModels.ApplicationReview;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Controllers
{
    public class ApplicationReviewControllerTests
    {
        private Fixture _fixture;
        private Mock<IApplicationReviewOrchestrator> _orchestrator;
        private ApplicationReviewController _controller;
        private Guid _vacancyId;
        private Guid _applicationReviewId;
        private long _ukprn;
        private string _candidateName;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _orchestrator = new Mock<IApplicationReviewOrchestrator>();
            _vacancyId = Guid.NewGuid();
            _applicationReviewId = Guid.NewGuid();
            _ukprn = 10000234;
            _candidateName = "Jack Sparrow";
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ProviderRecruitClaims.IdamsUserUkprnClaimsTypeIdentifier, _ukprn.ToString()),
            }));
            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            _controller = new ApplicationReviewController(_orchestrator.Object)
            {
                TempData = tempData
            };
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Test]
        public async Task POST_ApplicationReview_StatusInReview_RedirectsToVacancyManagePage()
        {
            // Arrange
            var editModel = _fixture.Build<ApplicationReviewEditModel>()
                .With(x => x.Outcome, ApplicationReviewStatus.InReview)
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.Ukprn, _ukprn)
                .Create();

            _orchestrator.Setup(o => o.PostApplicationReviewStatusChangeModelAsync(It.Is<ApplicationReviewEditModel>(y => y == editModel), It.IsAny<VacancyUser>()))
                .ReturnsAsync(_candidateName);

            // Act
            var redirectResult = await _controller.ApplicationReview(editModel) as RedirectToRouteResult;

            // Assert
            Assert.NotNull(redirectResult);
            Assert.AreEqual(RouteNames.VacancyManage_Get, redirectResult.RouteName);
            Assert.AreEqual(_vacancyId, redirectResult.RouteValues["VacancyId"]);
            Assert.AreEqual(_ukprn, redirectResult.RouteValues["Ukprn"]);
            Assert.IsTrue(_controller.TempData.ContainsKey(TempDataKeys.ApplicationStatusChangedHeader));
            Assert.AreEqual(string.Format(InfoMessages.ApplicationStatusChangeBannerHeader, _candidateName, editModel.Outcome.GetDisplayName().ToLower()), _controller.TempData[TempDataKeys.ApplicationStatusChangedHeader]);
        }


        [Test]
        public async Task POST_ApplicationReview_StatusInterviewing_RedirectsToVacancyManagePage()
        {
            // Arrange
            var editModel = _fixture.Build<ApplicationReviewEditModel>()
                .With(x => x.Outcome, ApplicationReviewStatus.Interviewing)
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.Ukprn, _ukprn)
                .Create();
            _orchestrator.Setup(o => o.PostApplicationReviewStatusChangeModelAsync(It.Is<ApplicationReviewEditModel>(y => y == editModel), It.IsAny<VacancyUser>()))
                .ReturnsAsync(_candidateName);

            // Act
            var redirectResult = await _controller.ApplicationReview(editModel) as RedirectToRouteResult;

            // Assert
            Assert.NotNull(redirectResult);
            Assert.AreEqual(RouteNames.VacancyManage_Get, redirectResult.RouteName);
            Assert.AreEqual(_vacancyId, redirectResult.RouteValues["VacancyId"]);
            Assert.AreEqual(_ukprn, redirectResult.RouteValues["Ukprn"]);
            Assert.IsTrue(_controller.TempData.ContainsKey(TempDataKeys.ApplicationStatusChangedHeader));
            Assert.AreEqual(string.Format(InfoMessages.ApplicationStatusChangeBannerHeader, _candidateName, editModel.Outcome.GetDisplayName().ToLower()), _controller.TempData[TempDataKeys.ApplicationStatusChangedHeader]);
        }

        [Test]
        public async Task POST_ApplicationReview_StatusSuccessful_RedirectsToApplicationReviewConfirmation()
        {
            // Arrange
            var editModel = _fixture.Build<ApplicationReviewEditModel>()
                .With(x => x.Outcome, ApplicationReviewStatus.Successful)
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.Ukprn, _ukprn)
                .With(x => x.ApplicationReviewId, _applicationReviewId)
                .Create();

            // Act
            var redirectResult = await _controller.ApplicationReview(editModel) as RedirectToRouteResult;

            // Assert
            Assert.NotNull(redirectResult);
            Assert.AreEqual(RouteNames.ApplicationReviewConfirmation_Get, redirectResult.RouteName);
            Assert.AreEqual(_vacancyId, redirectResult.RouteValues["VacancyId"]);
            Assert.AreEqual(_ukprn, redirectResult.RouteValues["Ukprn"]);
            Assert.AreEqual(_applicationReviewId, redirectResult.RouteValues["ApplicationReviewId"]);
        }

        [Test]
        public async Task POST_ApplicationReview_StatusUnsuccessful_RedirectsToApplicationReviewConfirmation()
        {
            // Arrange
            var editModel = _fixture.Build<ApplicationReviewEditModel>()
                .With(x => x.Outcome, ApplicationReviewStatus.Unsuccessful)
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.Ukprn, _ukprn)
                .With(x => x.ApplicationReviewId, _applicationReviewId)
                .Create();

            // Act
            var redirectResult = await _controller.ApplicationReview(editModel) as RedirectToRouteResult;

            // Assert
            Assert.NotNull(redirectResult);
            Assert.AreEqual(RouteNames.ApplicationReviewConfirmation_Get, redirectResult.RouteName);
            Assert.AreEqual(_vacancyId, redirectResult.RouteValues["VacancyId"]);
            Assert.AreEqual(_ukprn, redirectResult.RouteValues["Ukprn"]);
            Assert.AreEqual(_applicationReviewId, redirectResult.RouteValues["ApplicationReviewId"]);
        }

        [Test]
        public async Task POST_ApplicationReview_StatusShared_RedirectsToShareApplicationConfirmationAction()
        {
            // Arrange
            var editModel = _fixture.Build<ApplicationReviewEditModel>()
                .With(x => x.Outcome, ApplicationReviewStatus.Shared)
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.Ukprn, _ukprn)
                .With(x => x.ApplicationReviewId, _applicationReviewId)
                .Create();

            // Act
            var redirectResult = await _controller.ApplicationReview(editModel) as RedirectToRouteResult;

            // Assert
            Assert.NotNull(redirectResult);
            Assert.AreEqual(RouteNames.ApplicationReviewsToShareConfirmation_Get, redirectResult.RouteName);
            Assert.AreEqual(_vacancyId, redirectResult.RouteValues["VacancyId"]);
            Assert.AreEqual(_ukprn, redirectResult.RouteValues["Ukprn"]);
            Assert.NotNull(redirectResult.RouteValues["ApplicationsToShare"]);
            Assert.That(redirectResult.RouteValues["ApplicationsToShare"], Has.Count.EqualTo(1));
        }

        [Test]
        public async Task POST_ApplicationReview_DefaultCase_ReturnsCurrentView()
        {
            // Arrange
            var editModel = _fixture.Build<ApplicationReviewEditModel>()
                .With(x => x.Outcome, ApplicationReviewStatus.New)
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.Ukprn, _ukprn)
                .With(x => x.ApplicationReviewId, _applicationReviewId)
                .Create();
            _orchestrator.Setup(o => o.GetApplicationReviewViewModelAsync(It.Is<ApplicationReviewEditModel>(y => y == editModel)))
                .ReturnsAsync(new ApplicationReviewViewModel
                {
                    VacancyId = editModel.VacancyId,
                    Ukprn = editModel.Ukprn,
                    ApplicationReviewId = editModel.ApplicationReviewId,
                    Name = _candidateName
                });

            // Act
            var viewResult = await _controller.ApplicationReview(editModel) as ViewResult;

            // Assert
            var actual = viewResult.Model as ApplicationReviewViewModel;
            Assert.AreEqual(actual.Ukprn, editModel.Ukprn);
            Assert.AreEqual(actual.VacancyId, editModel.VacancyId);
            Assert.AreEqual(actual.ApplicationReviewId, editModel.ApplicationReviewId);
        }

        [Test]
        public async Task POST_ApplicationStatusConfirmation_RedirectsToManageApplicationsPage()
        {
            // Arrange
            var editModel = _fixture.Build<ApplicationReviewStatusConfirmationEditModel>()
                .With(x => x.Outcome, ApplicationReviewStatus.Successful)
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.Ukprn, _ukprn)
                .With(x => x.ApplicationReviewId, _applicationReviewId)
                .Create();

            // Act
            var redirectResult = await _controller.ApplicationStatusConfirmation(editModel) as RedirectToRouteResult;

            // Assert
            Assert.NotNull(redirectResult);
            Assert.AreEqual(RouteNames.VacancyManage_Get, redirectResult.RouteName);
            Assert.AreEqual(_vacancyId, redirectResult.RouteValues["VacancyId"]);
            Assert.AreEqual(_ukprn, redirectResult.RouteValues["Ukprn"]);
        }

        [Test]
        public async Task POST_ApplicationStatusConfirmation_StatusSuccessful_RedirectsToManageApplicationsPage()
        {
            //Arrange
            var editModel = _fixture.Build<ApplicationReviewStatusConfirmationEditModel>()
                .With(x => x.Outcome, ApplicationReviewStatus.Successful)
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.Ukprn, _ukprn)
                .With(x => x.ApplicationReviewId, _applicationReviewId)
                .Create();
            _orchestrator.Setup(o => o.PostApplicationReviewStatusChangeModelAsync(It.Is<ApplicationReviewStatusConfirmationEditModel>(y => y == editModel), It.IsAny<VacancyUser>()))
                .ReturnsAsync(_candidateName);

            // Act
            var redirectResult = await _controller.ApplicationStatusConfirmation(editModel) as RedirectToRouteResult;

            // Assert
            Assert.NotNull(redirectResult);
            Assert.AreEqual(RouteNames.VacancyManage_Get, redirectResult.RouteName);
            Assert.AreEqual(_vacancyId, redirectResult.RouteValues["VacancyId"]);
            Assert.AreEqual(_ukprn, redirectResult.RouteValues["Ukprn"]);
            Assert.IsTrue(_controller.TempData.ContainsKey(TempDataKeys.ApplicationReviewSuccessStatusInfoMessage));
            Assert.AreEqual(string.Format(InfoMessages.ApplicationReviewSuccessStatusHeader, _candidateName), _controller.TempData[TempDataKeys.ApplicationReviewSuccessStatusInfoMessage]);
        }

        [Test]
        public async Task POST_ApplicationStatusConfirmation_StatusUnSuccessful_RedirectsToManageApplicationsPage()
        {
            //Arrange
            var editModel = _fixture.Build<ApplicationReviewStatusConfirmationEditModel>()
                .With(x => x.Outcome, ApplicationReviewStatus.Unsuccessful)
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.Ukprn, _ukprn)
                .With(x => x.ApplicationReviewId, _applicationReviewId)
                .Create();
            _orchestrator.Setup(o => o.PostApplicationReviewStatusChangeModelAsync(It.Is<ApplicationReviewStatusConfirmationEditModel>(y => y == editModel), It.IsAny<VacancyUser>()))
                .ReturnsAsync(_candidateName);

            // Act
            var redirectResult = await _controller.ApplicationStatusConfirmation(editModel) as RedirectToRouteResult;

            // Assert
            Assert.NotNull(redirectResult);
            Assert.AreEqual(RouteNames.VacancyManage_Get, redirectResult.RouteName);
            Assert.AreEqual(_vacancyId, redirectResult.RouteValues["VacancyId"]);
            Assert.AreEqual(_ukprn, redirectResult.RouteValues["Ukprn"]);
            Assert.IsTrue(_controller.TempData.ContainsKey(TempDataKeys.ApplicationReviewUnsuccessStatusInfoMessage));
            Assert.AreEqual(string.Format(InfoMessages.ApplicationReviewUnsuccessStatusHeader, _candidateName), _controller.TempData[TempDataKeys.ApplicationReviewUnsuccessStatusInfoMessage]);
        }
    }
}