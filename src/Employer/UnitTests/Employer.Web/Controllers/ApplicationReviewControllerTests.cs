﻿using System.Security.Claims;
using Esfa.Recruit.Shared.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Controllers;
using Esfa.Recruit.Employer.Web.ViewModels.ApplicationReview;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.RouteModel;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Controllers
{
    public class ApplicationReviewControllerTests
    {
        private Fixture _fixture;
        private Mock<IApplicationReviewOrchestrator> _orchestrator;
        private ApplicationReviewController _controller;
        private Guid _vacancyId;
        private Guid _applicationReviewId;
        private ApplicationReviewCandidateInfo _candidateInfo;
        private string _employerAccountId;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _orchestrator = new Mock<IApplicationReviewOrchestrator>();
            _vacancyId = Guid.NewGuid();
            _applicationReviewId = Guid.NewGuid();
            _employerAccountId = "ADGFHAS";
            _candidateInfo = new ApplicationReviewCandidateInfo()
            {
                ApplicationReviewId = _applicationReviewId,
                Name = "Jack Sparrow",
                FriendlyId = "CASDFG3R"
            };
            var user = new ClaimsPrincipal(new ClaimsIdentity([
                new Claim(EmployerRecruitClaims.IdamsUserIdClaimTypeIdentifier, _applicationReviewId.ToString())
            ]));
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
        public async Task GET_ApplicationReview_ApplicationNew_CanShowRadioButtonReviewAndInterviewingTrue()
        {
            // Arrange
            var routeModel = _fixture.Build<ApplicationReviewRouteModel>()
                .With(x => x.ApplicationReviewId, _applicationReviewId)
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.EmployerAccountId, _employerAccountId)
                .Create();

            _orchestrator.Setup(o => o.GetApplicationReviewViewModelAsync(routeModel))
                .ReturnsAsync(new ApplicationReviewViewModel 
                {
                    ApplicationReviewId = _applicationReviewId,
                    VacancyId = _vacancyId,
                    EmployerAccountId = _employerAccountId,
                    Status = ApplicationReviewStatus.New
                });

            // Act
            var result = await _controller.ApplicationReview(routeModel) as ViewResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            var actual = result.Model as ApplicationReviewViewModel;
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual.VacancyId, Is.EqualTo(routeModel.VacancyId));
            Assert.That(actual.EmployerAccountId, Is.EqualTo(routeModel.EmployerAccountId));
            Assert.That(actual.CanShowRadioButtonReview, Is.True);
            Assert.That(actual.CanShowRadioButtonInterviewing, Is.True);
        }

        [Test]
        public async Task GET_ApplicationReview_ApplicationInReview_CanShowRadioButtonReviewFalseAndCanShowRadioButtonInterviewingTrue()
        {
            // Arrange
            var routeModel = _fixture.Build<ApplicationReviewRouteModel>()
                .With(x => x.ApplicationReviewId, _applicationReviewId)
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.EmployerAccountId, _employerAccountId)
                .Create();

            _orchestrator.Setup(o => o.GetApplicationReviewViewModelAsync(routeModel))
                .ReturnsAsync(new ApplicationReviewViewModel
                {
                    ApplicationReviewId = _applicationReviewId,
                    VacancyId = _vacancyId,
                    EmployerAccountId = _employerAccountId,
                    Status = ApplicationReviewStatus.InReview
                });

            // Act
            var result = await _controller.ApplicationReview(routeModel) as ViewResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            var actual = result.Model as ApplicationReviewViewModel;
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual.VacancyId, Is.EqualTo(routeModel.VacancyId));
            Assert.That(actual.EmployerAccountId, Is.EqualTo(routeModel.EmployerAccountId));
            Assert.That(actual.CanShowRadioButtonReview, Is.False);
            Assert.That(actual.CanShowRadioButtonInterviewing, Is.True);
        }

        [Test]
        public async Task GET_ApplicationReview_Applicationunsuccessful_CanShowRadioButtonReviewAndInterviewingFalse()
        {
            // Arrange
            var routeModel = _fixture.Build<ApplicationReviewRouteModel>()
                .With(x => x.ApplicationReviewId, _applicationReviewId)
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.EmployerAccountId, _employerAccountId)
                .Create();

            _orchestrator.Setup(o => o.GetApplicationReviewViewModelAsync(routeModel))
                .ReturnsAsync(new ApplicationReviewViewModel
                {
                    ApplicationReviewId = _applicationReviewId,
                    VacancyId = _vacancyId,
                    EmployerAccountId = _employerAccountId,
                    Status = ApplicationReviewStatus.Unsuccessful
                });

            // Act
            var result = await _controller.ApplicationReview(routeModel) as ViewResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            var actual = result.Model as ApplicationReviewViewModel;
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual.VacancyId, Is.EqualTo(routeModel.VacancyId));
            Assert.That(actual.EmployerAccountId, Is.EqualTo(routeModel.EmployerAccountId));
            Assert.That(actual.CanShowRadioButtonReview, Is.False);
            Assert.That(actual.CanShowRadioButtonInterviewing, Is.False);
        }

        [Test]
        public async Task POST_ApplicationReview_StatusEmployerInterviewing_RedirectsToVacancyManage()
        {
            // Arrange
            var editModel = _fixture.Build<ApplicationReviewEditModel>()
                .With(x => x.ApplicationReviewId, _applicationReviewId)
                .With(x => x.Outcome, ApplicationReviewStatus.EmployerInterviewing)
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.EmployerAccountId, _employerAccountId)
                .With(c=>c.IsApplicationSharedByProvider,true)
                .Create();

            _orchestrator.Setup(o => o.PostApplicationReviewEditModelAsync( editModel, It.IsAny<VacancyUser>()))
                .ReturnsAsync(_candidateInfo);

            // Act
            var redirectResult = await _controller.ApplicationReview(editModel) as RedirectToRouteResult;

            // Assert
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(RouteNames.VacancyManage_Get, Is.EqualTo(redirectResult.RouteName));
            Assert.That(_vacancyId, Is.EqualTo(redirectResult.RouteValues["VacancyId"]));
            Assert.That(_employerAccountId, Is.EqualTo(redirectResult.RouteValues["EmployerAccountId"]));
            Assert.That(_controller.TempData.ContainsKey(TempDataKeys.ApplicationReviewStatusInfoMessage), Is.True);
            Assert.That(string.Format(InfoMessages.ApplicationEmployerInterviewingHeader, _candidateInfo.FriendlyId, _candidateInfo.Name), Is.EqualTo(_controller.TempData[TempDataKeys.ApplicationReviewStatusInfoMessage]));
        }

        [Test]
        public async Task POST_ApplicationReview_StatusEmployerUnsuccessful_RedirectsToVacancyManage()
        {
            // Arrange
            var editModel = _fixture.Build<ApplicationReviewEditModel>()
                .With(x => x.ApplicationReviewId, _applicationReviewId)
                .With(x => x.Outcome, ApplicationReviewStatus.EmployerUnsuccessful)
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.EmployerAccountId, _employerAccountId)
                .With(c=>c.IsApplicationSharedByProvider,true)
                .Create();

            _orchestrator.Setup(o => o.PostApplicationReviewEditModelAsync( editModel, It.IsAny<VacancyUser>()))
                .ReturnsAsync(_candidateInfo);

            // Act
            var redirectResult = await _controller.ApplicationReview(editModel) as RedirectToRouteResult;

            // Assert
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(RouteNames.VacancyManage_Get, Is.EqualTo(redirectResult.RouteName));
            Assert.That(_vacancyId, Is.EqualTo(redirectResult.RouteValues["VacancyId"]));
            Assert.That(_employerAccountId, Is.EqualTo(redirectResult.RouteValues["EmployerAccountId"]));
            Assert.That(_controller.TempData.ContainsKey(TempDataKeys.ApplicationReviewStatusInfoMessage), Is.True);
            Assert.That(string.Format(InfoMessages.ApplicationEmployerUnsuccessfulHeader, _candidateInfo.FriendlyId), Is.EqualTo(_controller.TempData[TempDataKeys.ApplicationReviewStatusInfoMessage]));
        }

        [Test]
        public async Task POST_ApplicationReview_StatusUnsuccessful_RedirectsToApplicationReviewConfirmationView()
        {
            // Arrange
            var editModel = _fixture.Build<ApplicationReviewEditModel>()
                .With(x => x.ApplicationReviewId, _applicationReviewId)
                .With(x => x.Outcome, ApplicationReviewStatus.Unsuccessful)
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.EmployerAccountId, _employerAccountId)
                .Create();

            // Act
            var redirectResult = await _controller.ApplicationReview(editModel) as RedirectToRouteResult;

            // Assert
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(RouteNames.ApplicationReviewConfirmation_Get, Is.EqualTo(redirectResult.RouteName));
            Assert.That(_vacancyId, Is.EqualTo(redirectResult.RouteValues["VacancyId"]));
            Assert.That(_employerAccountId, Is.EqualTo(redirectResult.RouteValues["EmployerAccountId"]));
        }
      
        [Test] //This was never enabled
        public async Task POST_ApplicationReview_StatusInReview_RedirectsToVacancyManageWithCorrectTempDataMessage()
        {
            // Arrange
            var editModel = _fixture.Build<ApplicationReviewEditModel>()
                .With(x => x.ApplicationReviewId, _applicationReviewId)
                .With(x => x.Outcome, ApplicationReviewStatus.InReview)
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.EmployerAccountId, _employerAccountId)
                .With(x => x.CandidateFeedback, "feedback")
                .Create();

            _orchestrator.Setup(o => o.PostApplicationReviewConfirmationEditModelAsync(It.IsAny<ApplicationReviewStatusConfirmationEditModel>(), It.IsAny<VacancyUser>()))
                .ReturnsAsync(new ApplicationReviewStatusUpdateInfo
                {
                    CandidateName = _candidateInfo.Name,
                    ShouldMakeOthersUnsuccessful = false
                });

            // Act
            var redirectResult = await _controller.ApplicationReview(editModel) as RedirectToRouteResult;

            // Assert
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(RouteNames.VacancyManage_Get, Is.EqualTo(redirectResult.RouteName));
            Assert.That(_vacancyId, Is.EqualTo(redirectResult.RouteValues["VacancyId"]));
            Assert.That(_employerAccountId, Is.EqualTo(redirectResult.RouteValues["EmployerAccountId"]));
            Assert.That(_controller.TempData.ContainsKey(TempDataKeys.ApplicationReviewStatusChangeInfoMessage), Is.True);
        }

        [Test]
        public async Task POST_ApplicationStatusConfirmation_ShouldMakeOthersUnsuccessfulTrue_RedirectsToApplicationReviewsToUnsuccessfulPage()
        {
            // Arrange
            var editModel = _fixture.Build<ApplicationReviewStatusConfirmationEditModel>()
                .With(x => x.ApplicationReviewId, _applicationReviewId)
                .With(x => x.Outcome, ApplicationReviewStatus.Unsuccessful)
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.EmployerAccountId, _employerAccountId)
                .With(x => x.NotifyCandidate, true)
                .Create();

            _orchestrator.Setup(o => o.PostApplicationReviewConfirmationEditModelAsync(It.Is<ApplicationReviewStatusConfirmationEditModel>(y => y == editModel), It.IsAny<VacancyUser>()))
                .ReturnsAsync(new  ApplicationReviewStatusUpdateInfo
                    {
                        CandidateName = _candidateInfo.Name,
                        ShouldMakeOthersUnsuccessful = true,
                    });

            // Act
            var redirectResult = await _controller.ApplicationStatusConfirmation(editModel) as RedirectToRouteResult;

            // Assert
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(RouteNames.ApplicationReviewsToUnsuccessful_Get, Is.EqualTo(redirectResult.RouteName));
            Assert.That(_vacancyId, Is.EqualTo(redirectResult.RouteValues["VacancyId"]));
            Assert.That(_employerAccountId, Is.EqualTo(redirectResult.RouteValues["EmployerAccountId"]));
            Assert.That(_controller.TempData.ContainsKey(TempDataKeys.ApplicationReviewStatusInfoMessage), Is.True);
            Assert.That(string.Format(InfoMessages.ApplicationReviewSingleSuccessStatusHeader, _candidateInfo.Name), Is.EqualTo(_controller.TempData[TempDataKeys.ApplicationReviewStatusInfoMessage]));
        }

        [Test]
        public async Task POST_ApplicationStatusConfirmation_ShouldMakeOthersUnsuccessfulFalse_RedirectsToVacancyManagePage()
        {
            // Arrange
            var editModel = _fixture.Build<ApplicationReviewStatusConfirmationEditModel>()
                .With(x => x.ApplicationReviewId, _applicationReviewId)
                .With(x => x.Outcome, ApplicationReviewStatus.Unsuccessful)
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.EmployerAccountId, _employerAccountId)
                .With(x => x.NotifyCandidate, true)
                .Create();

            _orchestrator.Setup(o => o.PostApplicationReviewConfirmationEditModelAsync(It.Is<ApplicationReviewStatusConfirmationEditModel>(y => y == editModel), It.IsAny<VacancyUser>()))
                .ReturnsAsync(new ApplicationReviewStatusUpdateInfo
                {
                    CandidateName = _candidateInfo.Name,
                    ShouldMakeOthersUnsuccessful = false,
                });

            // Act
            var redirectResult = await _controller.ApplicationStatusConfirmation(editModel) as RedirectToRouteResult;
          
            // Assert
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(RouteNames.VacancyManage_Get, Is.EqualTo(redirectResult.RouteName));
            Assert.That(_vacancyId, Is.EqualTo(redirectResult.RouteValues["VacancyId"]));
            Assert.That(_employerAccountId, Is.EqualTo(redirectResult.RouteValues["EmployerAccountId"]));
            Assert.That(_controller.TempData.ContainsKey(TempDataKeys.ApplicationReviewStatusChangeInfoMessage), Is.True);
            Assert.That(string.Format(InfoMessages.ApplicationReviewStatusHeader, _candidateInfo.Name, editModel.Outcome.ToString().ToLower()), Is.EqualTo(_controller.TempData[TempDataKeys.ApplicationReviewStatusChangeInfoMessage]));
        }

        [Test]
        public async Task POST_ApplicationStatusConfirmation_NotifyCandidateFalse_RedirectsToVacancyManagePage()
        {
            // Arrange
            var editModel = _fixture.Build<ApplicationReviewStatusConfirmationEditModel>()
                .With(x => x.ApplicationReviewId, _applicationReviewId)
                .With(x => x.Outcome, ApplicationReviewStatus.Unsuccessful)
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.EmployerAccountId, _employerAccountId)
                .With(x => x.NotifyCandidate, false)
                .Create();

            _orchestrator.Setup(o => o.PostApplicationReviewConfirmationEditModelAsync(It.Is<ApplicationReviewStatusConfirmationEditModel>(y => y == editModel), It.IsAny<VacancyUser>()))
                .ReturnsAsync(new ApplicationReviewStatusUpdateInfo
                {
                    CandidateName = _candidateInfo.Name,
                    ShouldMakeOthersUnsuccessful = false,
                });

            // Act
            var redirectResult = await _controller.ApplicationStatusConfirmation(editModel) as RedirectToRouteResult;

            // Assert
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(RouteNames.ApplicationReview_Get, Is.EqualTo(redirectResult.RouteName));
            Assert.That(_vacancyId, Is.EqualTo(redirectResult.RouteValues["VacancyId"]));
            Assert.That(_employerAccountId, Is.EqualTo(redirectResult.RouteValues["EmployerAccountId"]));
        }
    }
}
