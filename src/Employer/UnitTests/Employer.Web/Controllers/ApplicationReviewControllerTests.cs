using System;
using System.Security.Claims;
using Esfa.Recruit.Shared.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using System.Threading.Tasks;
using AutoFixture;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Controllers;
using Esfa.Recruit.Employer.Web.ViewModels.ApplicationReview;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
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
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(EmployerRecruitClaims.IdamsUserIdClaimTypeIdentifier, _applicationReviewId.ToString()),
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
        public async Task GET_ApplicationReview_ApplicationNew_CanShowRadioButtonReviewAndInterviewingTrue()
        {
            // Arrange
            var expectedCanShowRadioButtonReview = true;
            var expectedCanShowRadioButtonInterviewing = true;
            var vacancySharedByProvider = false;
            var routeModel = _fixture.Build<ApplicationReviewRouteModel>()
                .With(x => x.ApplicationReviewId, _applicationReviewId)
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.EmployerAccountId, _employerAccountId)
                .Create();

            _orchestrator.Setup(o => o.GetApplicationReviewViewModelAsync(It.Is<ApplicationReviewRouteModel>(y => y == routeModel), vacancySharedByProvider))
                .ReturnsAsync(new ApplicationReviewViewModel 
                {
                    ApplicationReviewId = _applicationReviewId,
                    VacancyId = _vacancyId,
                    EmployerAccountId = _employerAccountId,
                    Status = ApplicationReviewStatus.New
                });

            // Act
            var result = await _controller.ApplicationReview(routeModel, vacancySharedByProvider) as ViewResult;

            // Assert
            var actual = result.Model as ApplicationReviewViewModel;
            Assert.AreEqual(actual.VacancyId, routeModel.VacancyId);
            Assert.AreEqual(actual.EmployerAccountId, routeModel.EmployerAccountId);
            Assert.AreEqual(expectedCanShowRadioButtonReview, actual.CanShowRadioButtonReview);
            Assert.AreEqual(expectedCanShowRadioButtonInterviewing, actual.CanShowRadioButtonInterviewing);
        }

        [Test]
        public async Task GET_ApplicationReview_ApplicationInReview_CanShowRadioButtonReviewFalseAndCanShowRadioButtonInterviewingTrue()
        {
            // Arrange
            var expectedCanShowRadioButtonReview = false;
            var expectedCanShowRadioButtonInterviewing = true;
            var vacancySharedByProvider = false;
            var routeModel = _fixture.Build<ApplicationReviewRouteModel>()
                .With(x => x.ApplicationReviewId, _applicationReviewId)
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.EmployerAccountId, _employerAccountId)
                .Create();

            _orchestrator.Setup(o => o.GetApplicationReviewViewModelAsync(It.Is<ApplicationReviewRouteModel>(y => y == routeModel), vacancySharedByProvider))
                .ReturnsAsync(new ApplicationReviewViewModel
                {
                    ApplicationReviewId = _applicationReviewId,
                    VacancyId = _vacancyId,
                    EmployerAccountId = _employerAccountId,
                    Status = ApplicationReviewStatus.InReview
                });

            // Act
            var result = await _controller.ApplicationReview(routeModel, vacancySharedByProvider) as ViewResult;

            // Assert
            var actual = result.Model as ApplicationReviewViewModel;
            Assert.AreEqual(actual.VacancyId, routeModel.VacancyId);
            Assert.AreEqual(actual.EmployerAccountId, routeModel.EmployerAccountId);
            Assert.AreEqual(expectedCanShowRadioButtonReview, actual.CanShowRadioButtonReview);
            Assert.AreEqual(expectedCanShowRadioButtonInterviewing, actual.CanShowRadioButtonInterviewing);
        }

        [Test]
        public async Task GET_ApplicationReview_Applicationunsuccessful_CanShowRadioButtonReviewAndInterviewingFalse()
        {
            // Arrange
            var expectedCanShowRadioButtonReview = false;
            var expectedCanShowRadioButtonInterviewing = false;
            var vacancySharedByProvider = false;
            var routeModel = _fixture.Build<ApplicationReviewRouteModel>()
                .With(x => x.ApplicationReviewId, _applicationReviewId)
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.EmployerAccountId, _employerAccountId)
                .Create();

            _orchestrator.Setup(o => o.GetApplicationReviewViewModelAsync(It.Is<ApplicationReviewRouteModel>(y => y == routeModel), vacancySharedByProvider))
                .ReturnsAsync(new ApplicationReviewViewModel
                {
                    ApplicationReviewId = _applicationReviewId,
                    VacancyId = _vacancyId,
                    EmployerAccountId = _employerAccountId,
                    Status = ApplicationReviewStatus.Unsuccessful
                });

            // Act
            var result = await _controller.ApplicationReview(routeModel, vacancySharedByProvider) as ViewResult;

            // Assert
            var actual = result.Model as ApplicationReviewViewModel;
            Assert.AreEqual(actual.VacancyId, routeModel.VacancyId);
            Assert.AreEqual(actual.EmployerAccountId, routeModel.EmployerAccountId);
            Assert.AreEqual(expectedCanShowRadioButtonReview, actual.CanShowRadioButtonReview);
            Assert.AreEqual(expectedCanShowRadioButtonInterviewing, actual.CanShowRadioButtonInterviewing);
        }

        [Test]
        public async Task POST_ApplicationReview_StatusEmployerInterviewing_RedirectsToVacancyManage()
        {
            // Arrange
            var vacancySharedByProvider = true;
            var editModel = _fixture.Build<ApplicationReviewEditModel>()
                .With(x => x.ApplicationReviewId, _applicationReviewId)
                .With(x => x.Outcome, ApplicationReviewStatus.EmployerInterviewing)
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.EmployerAccountId, _employerAccountId)
                .Create();

            _orchestrator.Setup(o => o.PostApplicationReviewEditModelAsync(It.Is<ApplicationReviewEditModel>(y => y == editModel), It.IsAny<VacancyUser>(), vacancySharedByProvider))
                .ReturnsAsync(_candidateInfo);

            // Act
            var redirectResult = await _controller.ApplicationReview(editModel, vacancySharedByProvider) as RedirectToRouteResult;

            // Assert
            Assert.NotNull(redirectResult);
            Assert.AreEqual(RouteNames.VacancyManage_Get, redirectResult.RouteName);
            Assert.AreEqual(_vacancyId, redirectResult.RouteValues["VacancyId"]);
            Assert.AreEqual(_employerAccountId, redirectResult.RouteValues["EmployerAccountId"]);
            Assert.IsTrue(_controller.TempData.ContainsKey(TempDataKeys.ApplicationReviewStatusInfoMessage));
            Assert.AreEqual(string.Format(InfoMessages.ApplicationEmployerInterviewingHeader, _candidateInfo.FriendlyId, _candidateInfo.Name), _controller.TempData[TempDataKeys.ApplicationReviewStatusInfoMessage]);
        }

        [Test]
        public async Task POST_ApplicationReview_StatusEmployerUnsuccessful_RedirectsToVacancyManage()
        {
            // Arrange
            var vacancySharedByProvider = true;
            var editModel = _fixture.Build<ApplicationReviewEditModel>()
                .With(x => x.ApplicationReviewId, _applicationReviewId)
                .With(x => x.Outcome, ApplicationReviewStatus.EmployerUnsuccessful)
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.EmployerAccountId, _employerAccountId)
                .Create();

            _orchestrator.Setup(o => o.PostApplicationReviewEditModelAsync(It.Is<ApplicationReviewEditModel>(y => y == editModel), It.IsAny<VacancyUser>(), vacancySharedByProvider))
                .ReturnsAsync(_candidateInfo);

            // Act
            var redirectResult = await _controller.ApplicationReview(editModel, vacancySharedByProvider) as RedirectToRouteResult;

            // Assert
            Assert.NotNull(redirectResult);
            Assert.AreEqual(RouteNames.VacancyManage_Get, redirectResult.RouteName);
            Assert.AreEqual(_vacancyId, redirectResult.RouteValues["VacancyId"]);
            Assert.AreEqual(_employerAccountId, redirectResult.RouteValues["EmployerAccountId"]);
            Assert.IsTrue(_controller.TempData.ContainsKey(TempDataKeys.ApplicationReviewStatusInfoMessage));
            Assert.AreEqual(string.Format(InfoMessages.ApplicationEmployerUnsuccessfulHeader, _candidateInfo.FriendlyId), _controller.TempData[TempDataKeys.ApplicationReviewStatusInfoMessage]);
        }

        [Test]
        public async Task POST_ApplicationReview_StatusUnsuccessful_RedirectsToApplicationReviewConfirmationView()
        {
            // Arrange
            var vacancySharedByProvider = false;
            var editModel = _fixture.Build<ApplicationReviewEditModel>()
                .With(x => x.ApplicationReviewId, _applicationReviewId)
                .With(x => x.Outcome, ApplicationReviewStatus.Unsuccessful)
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.EmployerAccountId, _employerAccountId)
                .Create();

            // Act
            var redirectResult = await _controller.ApplicationReview(editModel, vacancySharedByProvider) as RedirectToRouteResult;

            // Assert
            Assert.NotNull(redirectResult);
            Assert.AreEqual(RouteNames.ApplicationReviewConfirmation_Get, redirectResult.RouteName);
            Assert.AreEqual(_vacancyId, redirectResult.RouteValues["VacancyId"]);
            Assert.AreEqual(_employerAccountId, redirectResult.RouteValues["EmployerAccountId"]);
        }
      
        public async Task POST_ApplicationReview_StatusInReview_RedirectsToVacancyManageWithCorrectTempDataMessage()
        {
            // Arrange
            var vacancySharedByProvider = false;
            var editModel = _fixture.Build<ApplicationReviewEditModel>()
                .With(x => x.ApplicationReviewId, _applicationReviewId)
                .With(x => x.Outcome, ApplicationReviewStatus.InReview)
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.EmployerAccountId, _employerAccountId)
                .With(x => x.CandidateFeedback, "feedback")
                .Create();

            var confirmationEditModel = _fixture.Build<ApplicationReviewStatusConfirmationEditModel>()
                .With(x => x.ApplicationReviewId, editModel.ApplicationReviewId)
                .With(x => x.Outcome, editModel.Outcome)
                .With(x => x.VacancyId, editModel.VacancyId)
                .With(x => x.EmployerAccountId, editModel.EmployerAccountId)
                .With(x => x.CandidateFeedback, editModel.CandidateFeedback)
                .With(x => x.NotifyCandidate, false)
                .Create();

            _orchestrator.Setup(o => o.PostApplicationReviewConfirmationEditModelAsync(It.IsAny<ApplicationReviewStatusConfirmationEditModel>(), It.IsAny<VacancyUser>()))
                .ReturnsAsync(new ApplicationReviewStatusUpdateInfo
                {
                    CandidateName = _candidateInfo.Name,
                    ShouldMakeOthersUnsuccessful = false
                });

            // Act
            var redirectResult = await _controller.ApplicationReview(editModel, vacancySharedByProvider) as RedirectToRouteResult;

            // Assert
            Assert.NotNull(redirectResult);
            Assert.AreEqual(RouteNames.VacancyManage_Get, redirectResult.RouteName);
            Assert.AreEqual(_vacancyId, redirectResult.RouteValues["VacancyId"]);
            Assert.AreEqual(_employerAccountId, redirectResult.RouteValues["EmployerAccountId"]);
            Assert.IsTrue(_controller.TempData.ContainsKey(TempDataKeys.ApplicationReviewStatusChangeInfoMessage));
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
            Assert.NotNull(redirectResult);
            Assert.AreEqual(RouteNames.ApplicationReviewsToUnsuccessful_Get, redirectResult.RouteName);
            Assert.AreEqual(_vacancyId, redirectResult.RouteValues["VacancyId"]);
            Assert.AreEqual(_employerAccountId, redirectResult.RouteValues["EmployerAccountId"]);
            Assert.IsTrue(_controller.TempData.ContainsKey(TempDataKeys.ApplicationReviewStatusInfoMessage));
            Assert.AreEqual(string.Format(InfoMessages.ApplicationReviewSuccessStatusHeader, _candidateInfo.Name), _controller.TempData[TempDataKeys.ApplicationReviewStatusInfoMessage]);
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
            Assert.NotNull(redirectResult);
            Assert.AreEqual(RouteNames.VacancyManage_Get, redirectResult.RouteName);
            Assert.AreEqual(_vacancyId, redirectResult.RouteValues["VacancyId"]);
            Assert.AreEqual(_employerAccountId, redirectResult.RouteValues["EmployerAccountId"]);
            Assert.IsTrue(_controller.TempData.ContainsKey(TempDataKeys.ApplicationReviewStatusChangeInfoMessage));
            Assert.AreEqual(string.Format(InfoMessages.ApplicationReviewStatusHeader, _candidateInfo.Name, editModel.Outcome.ToString().ToLower()), _controller.TempData[TempDataKeys.ApplicationReviewStatusChangeInfoMessage]);
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
            Assert.NotNull(redirectResult);
            Assert.AreEqual(RouteNames.ApplicationReview_Get, redirectResult.RouteName);
            Assert.AreEqual(_vacancyId, redirectResult.RouteValues["VacancyId"]);
            Assert.AreEqual(_employerAccountId, redirectResult.RouteValues["EmployerAccountId"]);
        }
    }
}
