using System.Collections.Generic;
using System.Security.Claims;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Controllers;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Models;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.ApplicationReview;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;
using NUnit.Framework;
using ApplicationReviewViewModel = Esfa.Recruit.Provider.Web.ViewModels.ApplicationReview.ApplicationReviewViewModel;

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
                .ReturnsAsync(new ApplicationReviewStatusChangeInfo 
                { 
                    CandidateName = _candidateName,
                    ShouldMakeOthersUnsuccessful = false
                });

            // Act
            var redirectResult = await _controller.ApplicationReview(editModel) as RedirectToRouteResult;

            // Assert
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(RouteNames.VacancyManage_Get, Is.EqualTo(redirectResult.RouteName));
            Assert.That(_vacancyId, Is.EqualTo(redirectResult.RouteValues["VacancyId"]));
            Assert.That(_ukprn, Is.EqualTo(redirectResult.RouteValues["Ukprn"]));
            Assert.That(_controller.TempData.ContainsKey(TempDataKeys.ApplicationStatusChangedHeader), Is.True);
            Assert.That(string.Format(InfoMessages.ApplicationStatusChangeBannerHeader, _candidateName, editModel.Outcome.GetDisplayName().ToLower()), Is.EqualTo(_controller.TempData[TempDataKeys.ApplicationStatusChangedHeader]));
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
                .ReturnsAsync(new ApplicationReviewStatusChangeInfo
                {
                    CandidateName = _candidateName,
                    ShouldMakeOthersUnsuccessful = false
                });

            // Act
            var redirectResult = await _controller.ApplicationReview(editModel) as RedirectToRouteResult;

            // Assert
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(RouteNames.VacancyManage_Get, Is.EqualTo(redirectResult.RouteName));
            Assert.That(_vacancyId, Is.EqualTo(redirectResult.RouteValues["VacancyId"]));
            Assert.That(_ukprn, Is.EqualTo(redirectResult.RouteValues["Ukprn"]));
            Assert.That(_controller.TempData.ContainsKey(TempDataKeys.ApplicationStatusChangedHeader), Is.True);
            Assert.That(string.Format(InfoMessages.ApplicationStatusChangeBannerHeader, _candidateName, editModel.Outcome.GetDisplayName().ToLower()), Is.EqualTo(_controller.TempData[TempDataKeys.ApplicationStatusChangedHeader]));
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
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(RouteNames.ApplicationReviewConfirmation_Get, Is.EqualTo(redirectResult.RouteName));
            Assert.That(_vacancyId, Is.EqualTo(redirectResult.RouteValues["VacancyId"]));
            Assert.That(_ukprn, Is.EqualTo(redirectResult.RouteValues["Ukprn"]));
            Assert.That(_applicationReviewId, Is.EqualTo(redirectResult.RouteValues["ApplicationReviewId"]));
        }

        [Test]
        public async Task POST_ApplicationReview_StatusEmployerUnsuccessful_RedirectsToApplicationReviewConfirmation()
        {
            // Arrange
            var editModel = _fixture.Build<ApplicationReviewEditModel>()
                .With(x => x.Outcome, ApplicationReviewStatus.EmployerUnsuccessful)
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.Ukprn, _ukprn)
                .With(x => x.ApplicationReviewId, _applicationReviewId)
                .Create();

            // Act
            var redirectResult = await _controller.ApplicationReview(editModel) as RedirectToRouteResult;

            // Assert
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(RouteNames.ApplicationReviewConfirmation_Get, Is.EqualTo(redirectResult.RouteName));
            Assert.That(_vacancyId, Is.EqualTo(redirectResult.RouteValues["VacancyId"]));
            Assert.That(_ukprn, Is.EqualTo(redirectResult.RouteValues["Ukprn"]));
            Assert.That(_applicationReviewId, Is.EqualTo(redirectResult.RouteValues["ApplicationReviewId"]));
        }

        [Test]
        public async Task POST_ApplicationReview_StatusUnsuccessful_RedirectsToApplicationReviewFeedback()
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
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(RouteNames.ApplicationReviewFeedback_Get, Is.EqualTo(redirectResult.RouteName));
            Assert.That(_vacancyId, Is.EqualTo(redirectResult.RouteValues["VacancyId"]));
            Assert.That(_ukprn, Is.EqualTo(redirectResult.RouteValues["Ukprn"]));
            Assert.That(_applicationReviewId, Is.EqualTo(redirectResult.RouteValues["ApplicationReviewId"]));
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
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(RouteNames.ApplicationReviewsToShareConfirmation_Get, Is.EqualTo(redirectResult.RouteName));
            Assert.That(_vacancyId, Is.EqualTo(redirectResult.RouteValues["VacancyId"]));
            Assert.That(_ukprn, Is.EqualTo(redirectResult.RouteValues["Ukprn"]));
            Assert.That(redirectResult.RouteValues["ApplicationsToShare"], Is.Not.Null);
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
            Assert.That(actual.Ukprn, Is.EqualTo(editModel.Ukprn));
            Assert.That(actual.VacancyId, Is.EqualTo(editModel.VacancyId));
            Assert.That(actual.ApplicationReviewId, Is.EqualTo(editModel.ApplicationReviewId));
        }

        [Test]
        public async Task POST_ApplicationStatusConfirmation_StatusNew_RedirectsToManageVacancyPageWithDefaultTempData()
        {
            // Arrange
            var editModel = _fixture.Build<ApplicationReviewStatusConfirmationEditModel>()
                .With(x => x.Outcome, ApplicationReviewStatus.New)
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.Ukprn, _ukprn)
                .With(x => x.ApplicationReviewId, _applicationReviewId)
                .Create();
            _orchestrator.Setup(o => o.PostApplicationReviewStatusChangeModelAsync(It.Is<ApplicationReviewStatusConfirmationEditModel>(y => y == editModel), It.IsAny<VacancyUser>()))
                .ReturnsAsync(new ApplicationReviewStatusChangeInfo
                {
                    CandidateName = _candidateName,
                    ShouldMakeOthersUnsuccessful = false
                });

            // Act
            var redirectResult = await _controller.ApplicationStatusConfirmation(editModel) as RedirectToRouteResult;

            // Assert
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(RouteNames.VacancyManage_Get, Is.EqualTo(redirectResult.RouteName));
            Assert.That(_vacancyId, Is.EqualTo(redirectResult.RouteValues["VacancyId"]));
            Assert.That(_ukprn, Is.EqualTo(redirectResult.RouteValues["Ukprn"]));
            Assert.That(_controller.TempData.ContainsKey(TempDataKeys.ApplicationReviewStatusInfoMessage), Is.True);
            Assert.That(string.Format(InfoMessages.ApplicationReviewStatusHeader, _candidateName, editModel.Outcome.ToString().ToLower()), Is.EqualTo(_controller.TempData[TempDataKeys.ApplicationReviewStatusInfoMessage]));
        }

        [Test]
        public async Task POST_ApplicationStatusConfirmation_StatusSuccessful_RedirectsToManageVacancyPageWithTempData()
        {
            //Arrange
            var editModel = _fixture.Build<ApplicationReviewStatusConfirmationEditModel>()
                .With(x => x.Outcome, ApplicationReviewStatus.Successful)
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.Ukprn, _ukprn)
                .With(x => x.ApplicationReviewId, _applicationReviewId)
                .Create();
            _orchestrator.Setup(o => o.PostApplicationReviewStatusChangeModelAsync(It.Is<ApplicationReviewStatusConfirmationEditModel>(y => y == editModel), It.IsAny<VacancyUser>()))
                .ReturnsAsync(new ApplicationReviewStatusChangeInfo
                {
                    CandidateName = _candidateName,
                    ShouldMakeOthersUnsuccessful = false
                });

            // Act
            var redirectResult = await _controller.ApplicationStatusConfirmation(editModel) as RedirectToRouteResult;

            // Assert
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(RouteNames.VacancyManage_Get, Is.EqualTo(redirectResult.RouteName));
            Assert.That(_vacancyId, Is.EqualTo(redirectResult.RouteValues["VacancyId"]));
            Assert.That(_ukprn, Is.EqualTo(redirectResult.RouteValues["Ukprn"]));
            Assert.That(_controller.TempData.ContainsKey(TempDataKeys.ApplicationReviewSuccessStatusInfoMessage), Is.True);
            Assert.That(string.Format(InfoMessages.ApplicationReviewSingleSuccessStatusHeader, _candidateName), Is.EqualTo(_controller.TempData[TempDataKeys.ApplicationReviewSuccessStatusInfoMessage]));
        }

        [Test]
        public async Task POST_ApplicationStatusConfirmation_ShouldMakeOthersUnsuccessfulTrue_RedirectsToApplicationReviewsToUnsuccessfulPageWithTempData()
        {
            //Arrange
            var editModel = _fixture.Build<ApplicationReviewStatusConfirmationEditModel>()
                .With(x => x.Outcome, ApplicationReviewStatus.Successful)
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.Ukprn, _ukprn)
                .With(x => x.ApplicationReviewId, _applicationReviewId)
                .Create();
            _orchestrator.Setup(o => o.PostApplicationReviewStatusChangeModelAsync(It.Is<ApplicationReviewStatusConfirmationEditModel>(y => y == editModel), It.IsAny<VacancyUser>()))
                .ReturnsAsync(new ApplicationReviewStatusChangeInfo
                {
                    CandidateName = _candidateName,
                    ShouldMakeOthersUnsuccessful = true
                });

            // Act
            var redirectResult = await _controller.ApplicationStatusConfirmation(editModel) as RedirectToRouteResult;

            // Assert
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(RouteNames.ApplicationReviewsToUnsuccessful_Get, Is.EqualTo(redirectResult.RouteName));
            Assert.That(_vacancyId, Is.EqualTo(redirectResult.RouteValues["VacancyId"]));
            Assert.That(_ukprn, Is.EqualTo(redirectResult.RouteValues["Ukprn"]));
            Assert.That(_controller.TempData.ContainsKey(TempDataKeys.ApplicationReviewStatusInfoMessage), Is.True);
            Assert.That(string.Format(InfoMessages.ApplicationReviewSingleSuccessStatusHeader, _candidateName), Is.EqualTo(_controller.TempData[TempDataKeys.ApplicationReviewStatusInfoMessage]));
        }

        [Test]
        public async Task GET_ApplicationFeedback_WithTempData_ReturnsViewWithViewModel()
        {
            string TempDataARModel = "ApplicationReviewEditModel";

            var editModel = new ApplicationReviewEditModel
            {
                CandidateFeedback = "This is the candidate's feedback.",
                Outcome = ApplicationReviewStatus.Unsuccessful,
                ApplicationReviewId = Guid.NewGuid(),
                Ukprn = 123456,
                VacancyId = Guid.NewGuid()
            };

            var applicationReviewFeedbackViewModel = new ApplicationReviewFeedbackViewModel
            {
                Name = "John Doe"
            };

            var routeModel = new Mock<ApplicationReviewRouteModel>();

            _orchestrator.Setup(o => o.GetApplicationReviewFeedbackViewModelAsync(It.IsAny<ApplicationReviewEditModel>()))
                .ReturnsAsync(applicationReviewFeedbackViewModel);

            _controller.TempData[TempDataARModel] = JsonConvert.SerializeObject(editModel);

            var result = await _controller.ApplicationFeedback(routeModel.Object);

            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = (ViewResult)result;
            var viewModel = (ApplicationReviewFeedbackViewModel)viewResult.Model;

            Assert.That(applicationReviewFeedbackViewModel.CandidateFeedback, Is.EqualTo(viewModel.CandidateFeedback));
            Assert.That(applicationReviewFeedbackViewModel.ApplicationReviewId, Is.EqualTo(viewModel.ApplicationReviewId));
            Assert.That(applicationReviewFeedbackViewModel.Name, Is.EqualTo(viewModel.Name));
            Assert.That(applicationReviewFeedbackViewModel.Ukprn, Is.EqualTo(viewModel.Ukprn));
            Assert.That(applicationReviewFeedbackViewModel.VacancyId, Is.EqualTo(viewModel.VacancyId));
        }

        [Test]
        public async Task GET_ApplicationFeedback_WithoutTempData_ReturnsRedirectToRouteResult()
        {
            var applicationReviewFeedbackViewModel = new ApplicationReviewFeedbackViewModel
            {
                Name = "John Doe"
            };

            var routeModel = new ApplicationReviewRouteModel
            {
                ApplicationReviewId = Guid.NewGuid(),
                Ukprn = 123456,
                VacancyId = Guid.NewGuid()
            };

            _orchestrator.Setup(o => o.GetApplicationReviewFeedbackViewModelAsync(It.IsAny<ApplicationReviewEditModel>()))
                .ReturnsAsync(applicationReviewFeedbackViewModel);

            var result = await _controller.ApplicationFeedback(routeModel);

            Assert.That(result, Is.InstanceOf<RedirectToRouteResult>());
            var redirectToRouteResult = (RedirectToRouteResult)result;

            Assert.That(RouteNames.ApplicationReview_Get, Is.EqualTo(redirectToRouteResult.RouteName));
            Assert.That(routeModel.ApplicationReviewId, Is.EqualTo(redirectToRouteResult.RouteValues["ApplicationReviewId"]));
            Assert.That(routeModel.VacancyId, Is.EqualTo(redirectToRouteResult.RouteValues["VacancyId"]));
            Assert.That(routeModel.Ukprn, Is.EqualTo(redirectToRouteResult.RouteValues["Ukprn"]));
        }

        [Test]
        public async Task POST_ApplicationFeedback_InvalidModelState_PopulatesDisplayDataAndReturnsView()
        {
            // Arrange
            var request = _fixture
                .Build<ApplicationReviewFeedbackViewModel>()
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.Ukprn, _ukprn)
                .Create();

            var displayData = new Dictionary<string, string>
    {
        { "Name", "John Smith" },
        { "FriendlyId", "VAC001" }
    };

            _controller.ModelState.AddModelError("CandidateFeedback", "Required");

            _orchestrator.Setup(o => o.GetApplicationReviewFeedbackViewModelAsync(request))
                .ReturnsAsync(displayData);

            // Act
            var result = await _controller.ApplicationFeedback(request) as ViewResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Model, Is.EqualTo(request));
            Assert.That(request.Name, Is.EqualTo("John Smith"));
            Assert.That(request.FriendlyId, Is.EqualTo("VAC001"));
            _orchestrator.Verify(o => o.PostApplicationReviewStatusChangeModelAsync(
                It.IsAny<ApplicationReviewStatusChangeModel>(),
                It.IsAny<VacancyUser>()), Times.Never);
        }

        [Test]
        public async Task POST_ApplicationFeedback_ValidModel_PostsCorrectStatusChangeModel()
        {
            // Arrange
            var request = _fixture
                .Build<ApplicationReviewFeedbackViewModel>()
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.Ukprn, _ukprn)
                .Create();

            var statusChangeInfo = _fixture
                .Build<ApplicationReviewStatusChangeInfo>()
                .With(x => x.ShouldMakeOthersUnsuccessful, false)
                .Create();

            _orchestrator.Setup(o => o.PostApplicationReviewStatusChangeModelAsync(
                    It.IsAny<ApplicationReviewStatusChangeModel>(),
                    It.IsAny<VacancyUser>()))
                .ReturnsAsync(statusChangeInfo);

            _orchestrator.Setup(o => o.IsAllApplicationReviewsHasOutcomeAsync(_vacancyId))
                .ReturnsAsync(false);

            // Act
            await _controller.ApplicationFeedback(request);

            // Assert
            _orchestrator.Verify(o => o.PostApplicationReviewStatusChangeModelAsync(
                It.Is<ApplicationReviewStatusChangeModel>(m =>
                    m.CandidateFeedback == request.CandidateFeedback &&
                    m.IsApplicationSharedByProvider == request.IsApplicationSharedByProvider &&
                    m.Outcome == request.Outcome &&
                    m.ApplicationReviewId == request.ApplicationReviewId &&
                    m.VacancyId == request.VacancyId &&
                    m.Ukprn == request.Ukprn),
                It.IsAny<VacancyUser>()),
                Times.Once);
        }

        [Test]
        public async Task POST_ApplicationFeedback_ShouldMakeOthersUnsuccessful_RedirectsToApplicationReviewsToUnsuccessful()
        {
            // Arrange
            var request = _fixture
                .Build<ApplicationReviewFeedbackViewModel>()
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.Ukprn, _ukprn)
                .Create();

            var statusChangeInfo = _fixture
                .Build<ApplicationReviewStatusChangeInfo>()
                .With(x => x.ShouldMakeOthersUnsuccessful, true)
                .Create();

            _orchestrator.Setup(o => o.PostApplicationReviewStatusChangeModelAsync(
                    It.IsAny<ApplicationReviewStatusChangeModel>(),
                    It.IsAny<VacancyUser>()))
                .ReturnsAsync(statusChangeInfo);

            // Act
            var redirectResult = await _controller.ApplicationFeedback(request) as RedirectToRouteResult;

            // Assert
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(redirectResult.RouteName, Is.EqualTo(RouteNames.ApplicationReviewsToUnsuccessful_Get));
            Assert.That(redirectResult.RouteValues["VacancyId"], Is.EqualTo(_vacancyId));
            Assert.That(redirectResult.RouteValues["Ukprn"], Is.EqualTo(_ukprn));
            Assert.That(_controller.TempData.ContainsKey(TempDataKeys.ApplicationReviewStatusInfoMessage), Is.True);
            Assert.That(_controller.TempData[TempDataKeys.ApplicationReviewStatusInfoMessage],
                Is.EqualTo(InfoMessages.ApplicationProviderUnsuccessfulHeader));
            _orchestrator.Verify(o => o.IsAllApplicationReviewsHasOutcomeAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Test]
        public async Task POST_ApplicationFeedback_ShouldMakeOthersUnsuccessful_DoesNotCheckAllApplicationsOutcome()
        {
            // Arrange
            var request = _fixture
                .Build<ApplicationReviewFeedbackViewModel>()
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.Ukprn, _ukprn)
                .Create();

            var statusChangeInfo = _fixture
                .Build<ApplicationReviewStatusChangeInfo>()
                .With(x => x.ShouldMakeOthersUnsuccessful, true)
                .Create();

            _orchestrator.Setup(o => o.PostApplicationReviewStatusChangeModelAsync(
                    It.IsAny<ApplicationReviewStatusChangeModel>(),
                    It.IsAny<VacancyUser>()))
                .ReturnsAsync(statusChangeInfo);

            // Act
            await _controller.ApplicationFeedback(request);

            // Assert
            _orchestrator.Verify(o => o.IsAllApplicationReviewsHasOutcomeAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Test]
        public async Task POST_ApplicationFeedback_AllApplicationsHaveOutcome_RedirectsToArchiveVacancy()
        {
            // Arrange
            var request = _fixture
                .Build<ApplicationReviewFeedbackViewModel>()
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.Ukprn, _ukprn)
                .Create();

            var statusChangeInfo = _fixture
                .Build<ApplicationReviewStatusChangeInfo>()
                .With(x => x.ShouldMakeOthersUnsuccessful, false)
                .Create();

            _orchestrator.Setup(o => o.PostApplicationReviewStatusChangeModelAsync(
                    It.IsAny<ApplicationReviewStatusChangeModel>(),
                    It.IsAny<VacancyUser>()))
                .ReturnsAsync(statusChangeInfo);

            _orchestrator.Setup(o => o.IsAllApplicationReviewsHasOutcomeAsync(_vacancyId))
                .ReturnsAsync(true);

            // Act
            var redirectResult = await _controller.ApplicationFeedback(request) as RedirectToRouteResult;

            // Assert
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(redirectResult.RouteName, Is.EqualTo(RouteNames.ArchiveVacancy_Get));
            Assert.That(redirectResult.RouteValues["VacancyId"], Is.EqualTo(_vacancyId));
            Assert.That(redirectResult.RouteValues["Ukprn"], Is.EqualTo(_ukprn));
            Assert.That(_controller.TempData.ContainsKey(TempDataKeys.ArchiveVacancyInfoMessage), Is.True);
            Assert.That(_controller.TempData[TempDataKeys.ArchiveVacancyInfoMessage],
                Is.EqualTo(InfoMessages.VacancyApplicantsOutcomeNotified));
        }

        [Test]
        public async Task POST_ApplicationFeedback_NotAllApplicationsHaveOutcome_RedirectsToVacancyManage()
        {
            // Arrange
            var request = _fixture
                .Build<ApplicationReviewFeedbackViewModel>()
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.Ukprn, _ukprn)
                .Create();

            var statusChangeInfo = _fixture
                .Build<ApplicationReviewStatusChangeInfo>()
                .With(x => x.ShouldMakeOthersUnsuccessful, false)
                .Create();

            _orchestrator.Setup(o => o.PostApplicationReviewStatusChangeModelAsync(
                    It.IsAny<ApplicationReviewStatusChangeModel>(),
                    It.IsAny<VacancyUser>()))
                .ReturnsAsync(statusChangeInfo);

            _orchestrator.Setup(o => o.IsAllApplicationReviewsHasOutcomeAsync(_vacancyId))
                .ReturnsAsync(false);

            // Act
            var redirectResult = await _controller.ApplicationFeedback(request) as RedirectToRouteResult;

            // Assert
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(redirectResult.RouteName, Is.EqualTo(RouteNames.VacancyManage_Get));
            Assert.That(redirectResult.RouteValues["VacancyId"], Is.EqualTo(_vacancyId));
            Assert.That(redirectResult.RouteValues["Ukprn"], Is.EqualTo(_ukprn));
            Assert.That(_controller.TempData.ContainsKey(TempDataKeys.ApplicationReviewUnsuccessStatusInfoMessage), Is.True);
            Assert.That(_controller.TempData[TempDataKeys.ApplicationReviewUnsuccessStatusInfoMessage],
                Is.EqualTo(InfoMessages.ApplicationProviderUnsuccessfulHeader));
        }

        [Test]
        public async Task POST_ApplicationFeedback_NotAllApplicationsHaveOutcome_DoesNotSetArchiveTempData()
        {
            // Arrange
            var request = _fixture
                .Build<ApplicationReviewFeedbackViewModel>()
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.Ukprn, _ukprn)
                .Create();

            var statusChangeInfo = _fixture
                .Build<ApplicationReviewStatusChangeInfo>()
                .With(x => x.ShouldMakeOthersUnsuccessful, false)
                .Create();

            _orchestrator.Setup(o => o.PostApplicationReviewStatusChangeModelAsync(
                    It.IsAny<ApplicationReviewStatusChangeModel>(),
                    It.IsAny<VacancyUser>()))
                .ReturnsAsync(statusChangeInfo);

            _orchestrator.Setup(o => o.IsAllApplicationReviewsHasOutcomeAsync(_vacancyId))
                .ReturnsAsync(false);

            // Act
            await _controller.ApplicationFeedback(request);

            // Assert
            Assert.That(_controller.TempData.ContainsKey(TempDataKeys.ArchiveVacancyInfoMessage), Is.False);
            Assert.That(_controller.TempData.ContainsKey(TempDataKeys.ApplicationReviewStatusInfoMessage), Is.False);
        }
    }
}