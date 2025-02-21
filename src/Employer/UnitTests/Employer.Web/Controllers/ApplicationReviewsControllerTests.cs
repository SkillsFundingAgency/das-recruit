using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Esfa.Recruit.Employer.Web.Controllers;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.ViewModels.ApplicationReviews;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Shared.Web.ViewModels.Validations.Fluent;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Extensions;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Controllers
{
    public class ApplicationReviewsControllerTests
    {
        private Fixture _fixture;
        private Mock<IApplicationReviewsOrchestrator> _orchestrator;
        private ApplicationReviewsController _controller;
        private Guid _vacancyId;
        private Guid _applicationReviewId;
        private Guid _applicationReviewIdTwo;
        private string _employerAccountId;
        protected Mock<IProfanityListProvider> _mockProfanityListProvider;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _orchestrator = new Mock<IApplicationReviewsOrchestrator>();
            _vacancyId = Guid.NewGuid();
            _applicationReviewId = Guid.NewGuid();
            _applicationReviewIdTwo = Guid.NewGuid();
            _employerAccountId = "ADGFHAS";
            _mockProfanityListProvider = new Mock<IProfanityListProvider>();
            _mockProfanityListProvider.Setup(x => x.GetProfanityListAsync()).Returns(GetProfanityListAsync());
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(EmployerRecruitClaims.IdamsUserIdClaimTypeIdentifier, _applicationReviewId.ToString()),
            }));
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
            var applications = new List<ApplicationReview> { };
            applications.Add(applicationReview1);
            applications.Add(applicationReview2);
            applications.Add(applicationReview3);

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
            var applications = new List<ApplicationReview> { };
            applications.Add(applicationReview1);
            applications.Add(applicationReview2);
            applications.Add(applicationReview3);

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
            var listOfApplicationReviews = new List<Guid>();
            listOfApplicationReviews.Add(_applicationReviewId);
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
            var listOfApplicationReviews = new List<Guid>();
            listOfApplicationReviews.Add(_applicationReviewId);
            listOfApplicationReviews.Add(_applicationReviewIdTwo);
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
                    ApplicationsToUnsuccessful = applicationsToUnsuccessful
                });

            // Act
            var result = await _controller.ApplicationReviewsFeedback(routeModel) as ViewResult;

            // Assert
            var actual = result.Model as ApplicationReviewsFeedbackViewModel;
            Assert.That(actual.ApplicationsToUnsuccessful, Is.Not.Empty);
            Assert.That(actual.ApplicationsToUnsuccessful.Count(), Is.EqualTo(applicationsToUnsuccessful.Count));
            Assert.That(routeModel.VacancyId, Is.EqualTo(actual.VacancyId));
            Assert.That(routeModel.EmployerAccountId, Is.EqualTo(actual.EmployerAccountId));
            Assert.That("Give feedback to the unsuccessful applicants", Is.EqualTo(actual.ApplicationsToUnsuccessfulFeedbackHeaderTitle));
            Assert.That("Your feedback will be sent to all applicants you have selected as unsuccessful.", Is.EqualTo(actual.ApplicationsToUnsuccessfulFeedbackDescription)); 
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
            Assert.That("Give feedback to the unsuccessful applicant", Is.EqualTo(actual.ApplicationsToUnsuccessfulFeedbackHeaderTitle));
            Assert.That("Your feedback will be sent to the applicant you have selected as unsuccessful.", Is.EqualTo(actual.ApplicationsToUnsuccessfulFeedbackDescription));
        }

        [Test, MoqAutoData]
        public async Task POST_ApplicationReviewsFeedback_RedirectsToAction(
            List<VacancyApplication> listOfApplicationReviews)
        {
            // Arrange
            var request = _fixture
                .Build<ApplicationReviewsFeedbackViewModel>()
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.EmployerAccountId, _employerAccountId)
                .With(x => x.ApplicationsToUnsuccessful, listOfApplicationReviews)
                .Create();

            // Act
            var redirectResult = await _controller.ApplicationReviewsFeedback(request) as RedirectToRouteResult;

            // Assert
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(RouteNames.ApplicationReviewsToUnsuccessfulConfirmation_Get, Is.EqualTo(redirectResult.RouteName));
            Assert.That(_vacancyId, Is.EqualTo(redirectResult.RouteValues["VacancyId"]));
            Assert.That(_employerAccountId, Is.EqualTo(redirectResult.RouteValues["EmployerAccountId"]));
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
                .With(x=>x.ApplicationsToUnsuccessful, new List<VacancyApplication>{new VacancyApplication()})
                .Create();
            var validator = new ApplicationReviewsFeedbackModelValidator(_mockProfanityListProvider.Object);

            // Act
            var result = validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Count.Should().Be(1);
            result.Errors[0].ErrorMessage.Should().Be(ApplicationReviewValidator.CandidateFeedbackRequiredForSingleApplication);
        }

        [Test]
        public async Task GET_ApplicationReviewsToUnsuccessfulConfirmation_ReturnsViewModelWithMultipleApplicationsText()
        {
            // Arrange
            var listOfApplicationReviews = new List<Guid>();
            listOfApplicationReviews.Add(_applicationReviewId);
            listOfApplicationReviews.Add(_applicationReviewIdTwo);

            var vacancyApplication1 = _fixture.Create<VacancyApplication>();
            var vacancyApplication2 = _fixture.Create<VacancyApplication>();
            var vacancyApplications = new List<VacancyApplication> { };
            vacancyApplications.Add(vacancyApplication1);
            vacancyApplications.Add(vacancyApplication2);

            var routeModel = _fixture
                .Build<ApplicationReviewsToUnsuccessfulRouteModel>()
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.EmployerAccountId, _employerAccountId)
                .Create();

            _orchestrator.Setup(o =>
                    o.GetApplicationReviewsToUnsuccessfulConfirmationViewModelAsync(It.Is<ApplicationReviewsToUnsuccessfulRouteModel>(y => y == routeModel)))
                .ReturnsAsync(new ApplicationReviewsToUnsuccessfulConfirmationViewModel
                {
                    VacancyId = routeModel.VacancyId,
                    EmployerAccountId = routeModel.EmployerAccountId,
                    VacancyApplicationsToUnsuccessful = vacancyApplications
                });

            // Act
            var result = await _controller.ApplicationReviewsToUnsuccessfulConfirmation(routeModel) as ViewResult;

            // Assert
            var actual = result.Model as ApplicationReviewsToUnsuccessfulConfirmationViewModel;
            Assert.That(actual.VacancyApplicationsToUnsuccessful, Is.Not.Empty);
            Assert.That(actual.VacancyApplicationsToUnsuccessful.Count(), Is.EqualTo(2));
            Assert.That(routeModel.VacancyId, Is.EqualTo(actual.VacancyId));
            Assert.That(routeModel.EmployerAccountId, Is.EqualTo(actual.EmployerAccountId));
            Assert.That("Make multiple applications unsuccessful", Is.EqualTo(actual.ApplicationReviewsConfirmationHeaderTitle));
            Assert.That("You will make these applications unsuccessful:", Is.EqualTo(actual.ApplicationReviewsConfirmationHeaderDescription));
            Assert.That("These applicants will be notified with this message:", Is.EqualTo(actual.ApplicationsReviewsConfirmationNotificationMessage));
            Assert.That("Do you want to make these applications unsuccessful?", Is.EqualTo(actual.ApplicationsReviewsConfirmationLegendMessage));
        }

        [Test]
        public async Task GET_ApplicationReviewsToUnsuccessfulConfirmation_ReturnsViewModelWithSingleApplicationsText()
        {
            // Arrange
            var listOfApplicationReviews = new List<Guid>();
            listOfApplicationReviews.Add(_applicationReviewId);

            var vacancyApplication1 = _fixture.Create<VacancyApplication>();
            var vacancyApplications = new List<VacancyApplication> { };
            vacancyApplications.Add(vacancyApplication1);

            var routeModel = _fixture
                .Build<ApplicationReviewsToUnsuccessfulRouteModel>()
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.EmployerAccountId, _employerAccountId)
                .Create();

            _orchestrator.Setup(o =>
                    o.GetApplicationReviewsToUnsuccessfulConfirmationViewModelAsync(It.Is<ApplicationReviewsToUnsuccessfulRouteModel>(y => y == routeModel)))
                .ReturnsAsync(new ApplicationReviewsToUnsuccessfulConfirmationViewModel
                {
                    VacancyId = routeModel.VacancyId,
                    EmployerAccountId = routeModel.EmployerAccountId,
                    VacancyApplicationsToUnsuccessful = vacancyApplications
                });

            // Act
            var result = await _controller.ApplicationReviewsToUnsuccessfulConfirmation(routeModel) as ViewResult;

            // Assert
            var actual = result.Model as ApplicationReviewsToUnsuccessfulConfirmationViewModel;
            Assert.That(actual.VacancyApplicationsToUnsuccessful, Is.Not.Empty);
            Assert.That(actual.VacancyApplicationsToUnsuccessful.Count(), Is.EqualTo(1));
            Assert.That(routeModel.VacancyId, Is.EqualTo(actual.VacancyId));
            Assert.That(routeModel.EmployerAccountId, Is.EqualTo(actual.EmployerAccountId));
            Assert.That("Make application unsuccessful", Is.EqualTo(actual.ApplicationReviewsConfirmationHeaderTitle));
            Assert.That("You will make this application unsuccessful:", Is.EqualTo(actual.ApplicationReviewsConfirmationHeaderDescription));
            Assert.That("This applicant will be notified with this message:", Is.EqualTo(actual.ApplicationsReviewsConfirmationNotificationMessage));
            Assert.That("Do you want to make this application unsuccessful?", Is.EqualTo(actual.ApplicationsReviewsConfirmationLegendMessage));
        }

        [Test]
        public async Task POST_ApplicationReviewsToUnsuccessfulConfirmation_NoSelected_RedirectsToManageVacancy()
        {
            // Arrange
            var vacancyApplication1 = _fixture.Create<VacancyApplication>();
            var vacancyApplications = new List<VacancyApplication> { };
            vacancyApplications.Add(vacancyApplication1);
            var request = _fixture
                .Build<ApplicationReviewsToUnsuccessfulConfirmationViewModel>()
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.EmployerAccountId, _employerAccountId)
                .With(x => x.VacancyApplicationsToUnsuccessful, vacancyApplications)
                .With(x => x.ApplicationsUnsuccessfulConfirmed, false)
                .Create();

            // Act
            var redirectResult = await _controller.ApplicationReviewsToUnsuccessfulConfirmation(request) as RedirectToRouteResult;

            // Assert
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(RouteNames.VacancyManage_Get, Is.EqualTo(redirectResult.RouteName));
            Assert.That(_vacancyId, Is.EqualTo(redirectResult.RouteValues["VacancyId"]));
            Assert.That(_employerAccountId, Is.EqualTo(redirectResult.RouteValues["EmployerAccountId"]));
            Assert.That(_controller.TempData.ContainsKey(TempDataKeys.ApplicationReviewsUnsuccessfulInfoMessage), Is.False);
        }

        [Test]
        public async Task POST_ApplicationReviewsToUnsuccessfulConfirmation_YesSelected_SingleApplication_RedirectsToManageVacancy()
        {
            // Arrange
            var vacancyApplication1 = _fixture.Create<VacancyApplication>();
            var vacancyApplications = new List<VacancyApplication> { };
            vacancyApplications.Add(vacancyApplication1);
            var request = _fixture
                .Build<ApplicationReviewsToUnsuccessfulConfirmationViewModel>()
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.EmployerAccountId, _employerAccountId)
                .With(x => x.VacancyApplicationsToUnsuccessful, vacancyApplications)
                .With(x => x.ApplicationsUnsuccessfulConfirmed, true)
            .Create();

            _orchestrator.Setup(o =>
                    o.PostApplicationReviewsToUnsuccessfulAsync(It.Is<ApplicationReviewsToUnsuccessfulConfirmationViewModel>(y => y == request), It.IsAny<VacancyUser>()))
                .Returns(Task.CompletedTask);

            // Act
            var redirectResult = await _controller.ApplicationReviewsToUnsuccessfulConfirmation(request) as RedirectToRouteResult;

            // Assert
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(RouteNames.VacancyManage_Get, Is.EqualTo(redirectResult.RouteName));
            Assert.That(_vacancyId, Is.EqualTo(redirectResult.RouteValues["VacancyId"]));
            Assert.That(_employerAccountId, Is.EqualTo(redirectResult.RouteValues["EmployerAccountId"]));
            Assert.That(_controller.TempData.ContainsKey(TempDataKeys.ApplicationReviewsUnsuccessfulInfoMessage), Is.True);
            Assert.That(string.Format(InfoMessages.ApplicationReviewUnsuccessStatusHeader, vacancyApplication1.CandidateName), Is.EqualTo(_controller.TempData[TempDataKeys.ApplicationReviewsUnsuccessfulInfoMessage]));
        }

        [Test]
        public async Task POST_ApplicationReviewsToUnsuccessfulConfirmation_YesSelected_MultipleApplications_RedirectsToManageVacancy()
        {
            // Arrange
            var vacancyApplication1 = _fixture.Create<VacancyApplication>();
            var vacancyApplication2 = _fixture.Create<VacancyApplication>();
            var vacancyApplications = new List<VacancyApplication> { };
            vacancyApplications.Add(vacancyApplication1);
            vacancyApplications.Add(vacancyApplication2);
            var request = _fixture
                .Build<ApplicationReviewsToUnsuccessfulConfirmationViewModel>()
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.EmployerAccountId, _employerAccountId)
                .With(x => x.VacancyApplicationsToUnsuccessful, vacancyApplications)
                .With(x => x.ApplicationsUnsuccessfulConfirmed, true)
            .Create();

            _orchestrator.Setup(o =>
                    o.PostApplicationReviewsToUnsuccessfulAsync(It.Is<ApplicationReviewsToUnsuccessfulConfirmationViewModel>(y => y == request), It.IsAny<VacancyUser>()))
                .Returns(Task.CompletedTask);

            // Act
            var redirectResult = await _controller.ApplicationReviewsToUnsuccessfulConfirmation(request) as RedirectToRouteResult;

            // Assert
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(RouteNames.VacancyManage_Get, Is.EqualTo(redirectResult.RouteName));
            Assert.That(_vacancyId, Is.EqualTo(redirectResult.RouteValues["VacancyId"]));
            Assert.That(_employerAccountId, Is.EqualTo(redirectResult.RouteValues["EmployerAccountId"]));
            Assert.That(_controller.TempData.ContainsKey(TempDataKeys.ApplicationReviewsUnsuccessfulInfoMessage), Is.True);
            Assert.That(InfoMessages.ApplicationsToUnsuccessfulBannerHeader, Is.EqualTo(_controller.TempData[TempDataKeys.ApplicationReviewsUnsuccessfulInfoMessage]));
        }

        public Task<IEnumerable<string>> GetProfanityListAsync()
        {
            return Task.FromResult<IEnumerable<string>>(new[] { "bother", "dang", "balderdash", "drat" });
        }
    }
}