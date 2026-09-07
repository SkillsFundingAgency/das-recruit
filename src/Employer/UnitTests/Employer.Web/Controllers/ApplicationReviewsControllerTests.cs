using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Controllers;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.ApplicationReviews;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Shared.Web.ViewModels.Validations.Fluent;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Extensions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Controllers
{
    public class ApplicationReviewsControllerTests
    {
        private Fixture _fixture;
        private Mock<IApplicationReviewsOrchestrator> _orchestrator;
        private ApplicationReviewsController _controller;
        private Guid _vacancyId;
        private Guid _applicationReviewId;
        private string _employerAccountId;
        private Mock<IProfanityListProvider> _mockProfanityListProvider;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _orchestrator = new Mock<IApplicationReviewsOrchestrator>();
            _vacancyId = Guid.NewGuid();
            _applicationReviewId = Guid.NewGuid();
            _employerAccountId = "ADGFHAS";
            _mockProfanityListProvider = new Mock<IProfanityListProvider>();
            _mockProfanityListProvider.Setup(x => x.GetProfanityListAsync()).Returns(GetProfanityListAsync());
            var user = new ClaimsPrincipal(new ClaimsIdentity([
                new Claim(EmployerRecruitClaims.IdamsUserIdClaimTypeIdentifier, _applicationReviewId.ToString())
            ]));
            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            _controller = new ApplicationReviewsController(_orchestrator.Object)
            {
                TempData = tempData
            };
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Test]
        public async Task GET_ApplicationReviewsToUnsuccessful_ReturnsViewAndModelWith3SortedApplications()
        {
            // Arrange
            var routeModel = _fixture.Create<VacancyRouteModel>();
            var applicationReview1 = _fixture.Create<ApplicationReview>();
            var applicationReview2 = _fixture.Create<ApplicationReview>();
            var applicationReview3 = _fixture.Create<ApplicationReview>();
            var applications = new List<ApplicationReview>
            {
                applicationReview1,
                applicationReview2,
                applicationReview3
            };

            _orchestrator.Setup(o =>
                    o.GetApplicationReviewsToUnsuccessfulViewModelAsync(It.Is<VacancyRouteModel>(y => y == routeModel), It.Is<SortColumn>(x => x.Equals(SortColumn.DateApplied)), It.Is<SortOrder>(x => x.Equals(SortOrder.Descending))))
                .ReturnsAsync(new ApplicationReviewsToUnsuccessfulViewModel
                {
                    VacancyId = routeModel.VacancyId,
                    EmployerAccountId = routeModel.EmployerAccountId,
                    VacancyApplications = applications.AsQueryable().Sort(SortColumn.DateApplied, SortOrder.Descending, false).Select(c => (VacancyApplication)c).ToList()
                });

            // Act
            var result = await _controller.ApplicationReviewsToUnsuccessful(routeModel, "DateApplied", "Descending") as ViewResult;

            // Assert
            var actual = result.Model as ApplicationReviewsToUnsuccessfulViewModel;
            Assert.That(actual.VacancyApplications, Is.Not.Empty);
            Assert.That(actual.VacancyApplications.Count(), Is.EqualTo(3));
            Assert.That(actual.VacancyId, Is.EqualTo(routeModel.VacancyId));
            Assert.That(actual.EmployerAccountId, Is.EqualTo(routeModel.EmployerAccountId));
            Assert.That(actual.VacancyApplications[0].SubmittedDate, Is.GreaterThan(actual.VacancyApplications[1].SubmittedDate));
            Assert.That(actual.VacancyApplications[1].SubmittedDate, Is.GreaterThan(actual.VacancyApplications[2].SubmittedDate));
        }

        [Test]
        public async Task GET_ApplicationReviewsToUnsuccessful_ReturnsViewAndModelWithNoApplications()
        {
            // Arrange
            var routeModel = _fixture.Create<VacancyRouteModel>();

            _orchestrator.Setup(o =>
                    o.GetApplicationReviewsToUnsuccessfulViewModelAsync(It.Is<VacancyRouteModel>(y => y == routeModel), It.Is<SortColumn>(x => x.Equals(SortColumn.Name)), It.Is<SortOrder>(x => x.Equals(SortOrder.Ascending))))
                .ReturnsAsync(new ApplicationReviewsToUnsuccessfulViewModel
                {
                    VacancyId = routeModel.VacancyId,
                    EmployerAccountId = routeModel.EmployerAccountId,
                    VacancyApplications = new List<VacancyApplication>()
                });

            // Act
            var result = await _controller.ApplicationReviewsToUnsuccessful(routeModel, "Name", "Ascending") as ViewResult;

            // Assert
            var actual = result.Model as ApplicationReviewsToUnsuccessfulViewModel;
            Assert.That(actual.VacancyApplications, Is.Empty);
            Assert.That(actual.VacancyApplications.Count(), Is.EqualTo(0));
            Assert.That(actual.VacancyId, Is.EqualTo(routeModel.VacancyId));
            Assert.That(actual.EmployerAccountId, Is.EqualTo(routeModel.EmployerAccountId));
        }

        [Test]
        public async Task GET_ApplicationReviewsToUnsuccessful_InvalidEnums_ReturnsViewModelWithDefaultOrder()
        {
            // Arrange
            var routeModel = _fixture.Create<VacancyRouteModel>();
            var applicationReview1 = _fixture.Create<ApplicationReview>();
            var applicationReview2 = _fixture.Create<ApplicationReview>();
            var applicationReview3 = _fixture.Create<ApplicationReview>();
            var applications = new List<ApplicationReview>
            {
                applicationReview1,
                applicationReview2,
                applicationReview3
            };

            _orchestrator.Setup(o =>
            o.GetApplicationReviewsToUnsuccessfulViewModelAsync(It.Is<VacancyRouteModel>(y => y == routeModel), It.Is<SortColumn>(x => x.Equals(SortColumn.Default)), It.Is<SortOrder>(x => x.Equals(SortOrder.Default))))
                .ReturnsAsync(new ApplicationReviewsToUnsuccessfulViewModel
                {
                    VacancyId = routeModel.VacancyId,
                    EmployerAccountId = routeModel.EmployerAccountId,
                    VacancyApplications = applications.AsQueryable().Sort(SortColumn.Default, SortOrder.Default, false).Select(c => (VacancyApplication)c).ToList()
                });

            // Act
            var result = await _controller.ApplicationReviewsToUnsuccessful(routeModel, "InvalidSortColumn", "InvalidSortOrder") as ViewResult;

            // Assert
            var actual = result.Model as ApplicationReviewsToUnsuccessfulViewModel;
            Assert.That(actual.VacancyApplications, Is.Not.Empty);
            Assert.That(actual.VacancyApplications.Count(), Is.EqualTo(3));
            Assert.That(actual.VacancyId, Is.EqualTo(routeModel.VacancyId));
            Assert.That(actual.EmployerAccountId, Is.EqualTo(routeModel.EmployerAccountId));
            Assert.That(actual.VacancyApplications[1].SubmittedDate, Is.GreaterThan(actual.VacancyApplications[0].SubmittedDate));
            Assert.That(actual.VacancyApplications[2].SubmittedDate, Is.GreaterThan(actual.VacancyApplications[1].SubmittedDate));
        }

        [Test]
        public async Task POST_ApplicationReviewsToUnsuccessfulAsync_RedirectsToAction()
        {
            // Arrange
            var listOfApplicationReviews = new List<Guid> {_applicationReviewId};
            var request = _fixture
                .Build<ApplicationReviewsToUnsuccessfulViewModel>()
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.EmployerAccountId, _employerAccountId)
                .With(x => x.ApplicationsToUnsuccessful, listOfApplicationReviews)
                .With(x => x.SortColumn, SortColumn.Name)
                .With(x => x.SortOrder, SortOrder.Ascending)
                .Create();

            // Act
            var redirectResult = await _controller.ApplicationReviewsToUnsuccessfulAsync(request) as RedirectToRouteResult;

            // Assert
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(RouteNames.ApplicationReviewsToUnsuccessfulFeedback_Get, Is.EqualTo(redirectResult.RouteName));
            Assert.That(_vacancyId, Is.EqualTo(redirectResult.RouteValues["VacancyId"]));
            Assert.That(_employerAccountId, Is.EqualTo(redirectResult.RouteValues["EmployerAccountId"]));
        }

        [Test]
        public async Task GET_ApplicationReviewsFeedback_ReturnsViewAndModelWithMultipleApplicationsText()
        {
            // Arrange
            var applicationsToUnsuccessful = _fixture.CreateMany<VacancyApplication>().ToList();
            var routeModel = _fixture
                .Build<ApplicationReviewsToUnsuccessfulRouteModel>()
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.EmployerAccountId, _employerAccountId)
                .Create();

            _orchestrator.Setup(o =>
                    o.GetApplicationReviewsFeedbackViewModel(It.Is<ApplicationReviewsToUnsuccessfulRouteModel>(y => y == routeModel)))
                .ReturnsAsync(new ApplicationReviewsFeedbackViewModel
                {
                    VacancyId = routeModel.VacancyId,
                    EmployerAccountId = routeModel.EmployerAccountId,
                    ApplicationsToUnsuccessful = applicationsToUnsuccessful,
                    IsMultipleApplications = true,
                });

            // Act
            var result = await _controller.ApplicationReviewsFeedback(routeModel) as ViewResult;

            // Assert
            var actual = result.Model as ApplicationReviewsFeedbackViewModel;
            Assert.That(actual.ApplicationsToUnsuccessful, Is.Not.Empty);
            Assert.That(actual.ApplicationsToUnsuccessful.Count(), Is.EqualTo(applicationsToUnsuccessful.Count));
            Assert.That(routeModel.VacancyId, Is.EqualTo(actual.VacancyId));
            Assert.That(routeModel.EmployerAccountId, Is.EqualTo(actual.EmployerAccountId));
        }

        [Test]
        public async Task GET_ApplicationReviewsFeedback_ReturnsViewAndModelWithSingleApplicationsText()
        {
            // Arrange
            var applicationsToUnsuccessful = _fixture.Create<VacancyApplication>();
            var routeModel = _fixture
                .Build<ApplicationReviewsToUnsuccessfulRouteModel>()
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.EmployerAccountId, _employerAccountId)
                .Create();

            _orchestrator.Setup(o =>
                    o.GetApplicationReviewsFeedbackViewModel(It.Is<ApplicationReviewsToUnsuccessfulRouteModel>(y => y == routeModel)))
                .ReturnsAsync(new ApplicationReviewsFeedbackViewModel
                {
                    VacancyId = routeModel.VacancyId,
                    EmployerAccountId = routeModel.EmployerAccountId,
                    ApplicationsToUnsuccessful = [applicationsToUnsuccessful]
                });

            // Act
            var result = await _controller.ApplicationReviewsFeedback(routeModel) as ViewResult;

            // Assert
            var actual = result.Model as ApplicationReviewsFeedbackViewModel;
            Assert.That(actual.ApplicationsToUnsuccessful, Is.Not.Empty);
            Assert.That(actual.ApplicationsToUnsuccessful.Count, Is.EqualTo(1));
            Assert.That(routeModel.VacancyId, Is.EqualTo(actual.VacancyId));
            Assert.That(routeModel.EmployerAccountId, Is.EqualTo(actual.EmployerAccountId));
        }

        [Test]
        public async Task POST_ApplicationReviewsFeedback_InvalidModelState_ReturnsView()
        {
            // Arrange
            var request = _fixture
                .Build<ApplicationReviewsFeedbackViewModel>()
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.EmployerAccountId, _employerAccountId)
                .Create();

            _controller.ModelState.AddModelError("CandidateFeedback", "Required");

            // Act
            var result = await _controller.ApplicationReviewsFeedback(request) as ViewResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Model, Is.EqualTo(request));
            _orchestrator.Verify(o => o.PostApplicationReviewsToUnsuccessfulAsync(
                It.IsAny<ApplicationReviewsToUnsuccessfulConfirmationViewModel>(),
                It.IsAny<VacancyUser>()), Times.Never);
        }

        [Test]
        public async Task POST_ApplicationReviewsFeedback_ValidModel_PostsCorrectConfirmationModel()
        {
            // Arrange
            var vacancyApplications = _fixture.CreateMany<VacancyApplication>(2).ToList();
            var request = _fixture
                .Build<ApplicationReviewsFeedbackViewModel>()
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.EmployerAccountId, _employerAccountId)
                .With(x => x.ApplicationsToUnsuccessful, vacancyApplications)
                .Create();

            _orchestrator.Setup(o => o.PostApplicationReviewsToUnsuccessfulAsync(
                    It.IsAny<ApplicationReviewsToUnsuccessfulConfirmationViewModel>(),
                    It.IsAny<VacancyUser>()))
                .Returns(Task.CompletedTask);

            _orchestrator.Setup(o => o.IsAllApplicationReviewsHasOutcomeAsync(_vacancyId))
                .ReturnsAsync(false);

            // Act
            await _controller.ApplicationReviewsFeedback(request);

            // Assert
            _orchestrator.Verify(o => o.PostApplicationReviewsToUnsuccessfulAsync(
                It.Is<ApplicationReviewsToUnsuccessfulConfirmationViewModel>(m =>
                    m.CandidateFeedback == request.CandidateFeedback &&
                    m.ApplicationsUnsuccessfulConfirmed == true &&
                    m.VacancyApplicationsToUnsuccessful == request.ApplicationsToUnsuccessful &&
                    m.Outcome == request.Outcome &&
                    m.VacancyId == request.VacancyId &&
                    m.EmployerAccountId == request.EmployerAccountId),
                It.IsAny<VacancyUser>()),
                Times.Once);
        }

        [Test]
        public async Task POST_ApplicationReviewsFeedback_AllApplicationsHaveOutcome_RedirectsToArchiveVacancy()
        {
            // Arrange
            var vacancyApplications = _fixture.CreateMany<VacancyApplication>(1).ToList();
            var request = _fixture
                .Build<ApplicationReviewsFeedbackViewModel>()
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.EmployerAccountId, _employerAccountId)
                .With(x => x.ApplicationsToUnsuccessful, vacancyApplications)
                .Create();

            _orchestrator.Setup(o => o.PostApplicationReviewsToUnsuccessfulAsync(
                    It.IsAny<ApplicationReviewsToUnsuccessfulConfirmationViewModel>(),
                    It.IsAny<VacancyUser>()))
                .Returns(Task.CompletedTask);

            _orchestrator.Setup(o => o.IsAllApplicationReviewsHasOutcomeAsync(_vacancyId))
                .ReturnsAsync(true);

            // Act
            var redirectResult = await _controller.ApplicationReviewsFeedback(request) as RedirectToRouteResult;

            // Assert
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(redirectResult.RouteName, Is.EqualTo(RouteNames.ArchiveVacancy_Get));
            Assert.That(redirectResult.RouteValues["VacancyId"], Is.EqualTo(_vacancyId));
            Assert.That(redirectResult.RouteValues["EmployerAccountId"], Is.EqualTo(_employerAccountId));
            Assert.That(_controller.TempData.ContainsKey(TempDataKeys.ArchiveAdvertInfoMessage), Is.True);
            Assert.That(_controller.TempData[TempDataKeys.ArchiveAdvertInfoMessage], Is.EqualTo(InfoMessages.AdvertApplicantsOutcomeNotified));
        }

        [Test]
        public async Task POST_ApplicationReviewsFeedback_NotAllApplicationsHaveOutcome_SingleApplication_RedirectsToVacancyManage()
        {
            // Arrange
            var vacancyApplications = _fixture.CreateMany<VacancyApplication>(1).ToList();
            var request = _fixture
                .Build<ApplicationReviewsFeedbackViewModel>()
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.EmployerAccountId, _employerAccountId)
                .With(x => x.ApplicationsToUnsuccessful, vacancyApplications)
                .Create();

            _orchestrator.Setup(o => o.PostApplicationReviewsToUnsuccessfulAsync(
                    It.IsAny<ApplicationReviewsToUnsuccessfulConfirmationViewModel>(),
                    It.IsAny<VacancyUser>()))
                .Returns(Task.CompletedTask);

            _orchestrator.Setup(o => o.IsAllApplicationReviewsHasOutcomeAsync(_vacancyId))
                .ReturnsAsync(false);

            // Act
            var redirectResult = await _controller.ApplicationReviewsFeedback(request) as RedirectToRouteResult;

            // Assert
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(redirectResult.RouteName, Is.EqualTo(RouteNames.VacancyManage_Get));
            Assert.That(redirectResult.RouteValues["VacancyId"], Is.EqualTo(_vacancyId));
            Assert.That(redirectResult.RouteValues["EmployerAccountId"], Is.EqualTo(_employerAccountId));
            Assert.That(_controller.TempData.ContainsKey(TempDataKeys.ArchiveAdvertInfoMessage), Is.False);
        }

        [Test]
        public async Task POST_ApplicationReviewsFeedback_NotAllApplicationsHaveOutcome_MultipleApplications_RedirectsToVacancyManage()
        {
            // Arrange
            var vacancyApplications = _fixture.CreateMany<VacancyApplication>(2).ToList();
            var request = _fixture
                .Build<ApplicationReviewsFeedbackViewModel>()
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.EmployerAccountId, _employerAccountId)
                .With(x => x.ApplicationsToUnsuccessful, vacancyApplications)
                .Create();

            _orchestrator.Setup(o => o.PostApplicationReviewsToUnsuccessfulAsync(
                    It.IsAny<ApplicationReviewsToUnsuccessfulConfirmationViewModel>(),
                    It.IsAny<VacancyUser>()))
                .Returns(Task.CompletedTask);

            _orchestrator.Setup(o => o.IsAllApplicationReviewsHasOutcomeAsync(_vacancyId))
                .ReturnsAsync(false);

            // Act
            var redirectResult = await _controller.ApplicationReviewsFeedback(request) as RedirectToRouteResult;

            // Assert
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(redirectResult.RouteName, Is.EqualTo(RouteNames.VacancyManage_Get));
            Assert.That(redirectResult.RouteValues["VacancyId"], Is.EqualTo(_vacancyId));
            Assert.That(redirectResult.RouteValues["EmployerAccountId"], Is.EqualTo(_employerAccountId));
            Assert.That(_controller.TempData.ContainsKey(TempDataKeys.ArchiveAdvertInfoMessage), Is.False);
        }

        [Test]
        public void POST_ApplicationReviewsFeedback_NoCandidateFeedbackValidationError()
        {
            // Arrange
            var request = _fixture
                .Build<ApplicationReviewsFeedbackViewModel>()
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.EmployerAccountId, _employerAccountId)
                .With(x => x.Outcome, ApplicationReviewStatus.Unsuccessful)
                .With(x=>x.CandidateFeedback, "")
                .With(x=>x.ApplicationsToUnsuccessful, [new VacancyApplication()])
                .With(x=>x.IsMultipleApplications, false)
                .Create();
            var validator = new ApplicationReviewsFeedbackModelValidator(_mockProfanityListProvider.Object);

            // Act
            var result = validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Count.Should().Be(1);
            result.Errors[0].ErrorMessage.Should().Be(ApplicationReviewValidator.CandidateFeedbackRequiredForSingleApplication);
        }

        private static Task<IEnumerable<string>> GetProfanityListAsync() => 
            Task.FromResult<IEnumerable<string>>(["bother", "dang", "balderdash", "drat"]);
    }
}