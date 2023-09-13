using System.Threading.Tasks;
using AutoFixture;
using Esfa.Recruit.Provider.Web.Controllers;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.ApplicationReviews;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;
using System.Collections.Generic;
using System.Linq;
using System;
using Esfa.Recruit.Provider.Web.Models.ApplicationReviews;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Provider.Web.Configuration;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Net.Http;
using Microsoft.Azure.Amqp.Transaction;
using Esfa.Recruit.Shared.Web.ViewModels;
using static Esfa.Recruit.Vacancies.Client.Application.Communications.CommunicationConstants.DataItemKeys;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Extensions;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Controllers
{
    public class ApplicationReviewsControllerTests
    {
        private Fixture _fixture;
        private Mock<IApplicationReviewsOrchestrator> _orchestrator;
        private ApplicationReviewsController _controller;
        private Guid _vacancyId;
        private long _ukprn;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _orchestrator = new Mock<IApplicationReviewsOrchestrator>();
            _vacancyId = Guid.NewGuid();
            _ukprn = 10000234;
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ProviderRecruitClaims.IdamsUserUkprnClaimsTypeIdentifier, _ukprn.ToString()),
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
            var applicationReviews = CreateApplicationReviewsFixture(3);
            var sortOrder = SortOrder.Descending;
            var sortColumn = SortColumn.DateApplied;

            _orchestrator.Setup(o =>
                    o.GetApplicationReviewsToUnsuccessfulViewModelAsync(It.Is<VacancyRouteModel>(y => y == routeModel), It.Is<SortColumn>(x => x.Equals(sortColumn)), It.Is<SortOrder>(x => x.Equals(sortOrder))))
                .ReturnsAsync(new ApplicationReviewsToUnsuccessfulViewModel
                {
                    VacancyId = routeModel.VacancyId,
                    Ukprn = routeModel.Ukprn,
                    VacancyApplications = applicationReviews.Sort(sortColumn, sortOrder, false).Select(c => (VacancyApplication)c).ToList()
                });

            // Act
            var result = await _controller.ApplicationReviewsToUnsuccessful(routeModel, "DateApplied", "Descending") as ViewResult;

            // Assert
            var actual = result.Model as ApplicationReviewsToUnsuccessfulViewModel;
            Assert.IsNotEmpty(actual.VacancyApplications);
            Assert.That(actual.VacancyApplications.Count(), Is.EqualTo(3));
            Assert.AreEqual(actual.Ukprn, routeModel.Ukprn);
            Assert.AreEqual(actual.VacancyId, routeModel.VacancyId);
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
                    Ukprn = routeModel.Ukprn,
                    VacancyApplications = new List<VacancyApplication>()
                });

            // Act
            var result = await _controller.ApplicationReviewsToUnsuccessful(routeModel, "Name", "Ascending") as ViewResult;

            // Assert
            var actual = result.Model as ApplicationReviewsToUnsuccessfulViewModel;
            Assert.IsEmpty(actual.VacancyApplications);
            Assert.That(actual.VacancyApplications.Count(), Is.EqualTo(0));
            Assert.AreEqual(actual.Ukprn, routeModel.Ukprn);
            Assert.AreEqual(actual.VacancyId, routeModel.VacancyId);
        }

        [Test]
        public async Task GET_ApplicationReviewsToUnsuccessful_SetsViewModelBannerViaTempData()
        {
            // Arrange
            var routeModel = _fixture.Create<VacancyRouteModel>();
            var vacancyApplication1 = _fixture.Create<VacancyApplication>();
            var vacancyApplication2 = _fixture.Create<VacancyApplication>();
            var vacancyApplications = new List<VacancyApplication> { };
            vacancyApplications.Add(vacancyApplication1);
            vacancyApplications.Add(vacancyApplication2);
            _controller.TempData.Add(TempDataKeys.ApplicationReviewStatusInfoMessage, string.Format(InfoMessages.ApplicationReviewSuccessStatusHeader, "Jack Sparrow"));
            var sortOrder = SortOrder.Descending;
            var sortColumn = SortColumn.Name;

            _orchestrator.Setup(o =>
                    o.GetApplicationReviewsToUnsuccessfulViewModelAsync(It.Is<VacancyRouteModel>(y => y == routeModel), It.Is<SortColumn>(x => x.Equals(sortColumn)), It.Is<SortOrder>(x => x.Equals(sortOrder))))
                .ReturnsAsync(new ApplicationReviewsToUnsuccessfulViewModel
                {
                    VacancyId = routeModel.VacancyId,
                    Ukprn = routeModel.Ukprn,
                    VacancyApplications = vacancyApplications
                });

            // Act
            var result = await _controller.ApplicationReviewsToUnsuccessful(routeModel, sortColumn.ToString(), sortOrder.ToString()) as ViewResult;

            // Assert
            var actual = result.Model as ApplicationReviewsToUnsuccessfulViewModel;
            Assert.IsNotEmpty(actual.VacancyApplications);
            Assert.That(actual.VacancyApplications.Count(), Is.EqualTo(2));
            Assert.AreEqual(actual.Ukprn, routeModel.Ukprn);
            Assert.AreEqual(actual.VacancyId, routeModel.VacancyId);
            Assert.AreEqual(actual.ShouldMakeOthersUnsuccessfulBannerHeader, _controller.TempData[TempDataKeys.ApplicationReviewStatusInfoMessage].ToString());
            Assert.AreEqual(actual.ShouldMakeOthersUnsuccessfulBannerBody, InfoMessages.ApplicationReviewSuccessStatusBannerMessage);
        }

        [Test]
        public async Task GET_ApplicationReviewsToUnsuccessful_InvalidEnumValues_DefaultSortingAppliedAscendingSubmittedDate()
        {
            // Arrange
            var routeModel = _fixture.Create<VacancyRouteModel>();
            var applicationReviews = CreateApplicationReviewsFixture(3);

            _orchestrator.Setup(o =>
                    o.GetApplicationReviewsToUnsuccessfulViewModelAsync(It.Is<VacancyRouteModel>(y => y == routeModel), It.Is<SortColumn>(x => x.Equals(SortColumn.Default)), It.Is<SortOrder>(x => x.Equals(SortOrder.Default))))
                .ReturnsAsync(new ApplicationReviewsToUnsuccessfulViewModel
                {
                    VacancyId = routeModel.VacancyId,
                    Ukprn = routeModel.Ukprn,
                    VacancyApplications = applicationReviews.Sort(SortColumn.Default, SortOrder.Default, false).Select(c => (VacancyApplication)c).ToList()
                });

            // Act
            var result = await _controller.ApplicationReviewsToUnsuccessful(routeModel, "InvalidSortColumn", "InvalidSortOrder") as ViewResult;

            // Assert
            var actual = result.Model as ApplicationReviewsToUnsuccessfulViewModel;
            Assert.IsNotEmpty(actual.VacancyApplications);
            Assert.That(actual.VacancyApplications.Count(), Is.EqualTo(3));
            Assert.AreEqual(actual.Ukprn, routeModel.Ukprn);
            Assert.AreEqual(actual.VacancyId, routeModel.VacancyId);
            Assert.That(actual.VacancyApplications[1].SubmittedDate, Is.GreaterThan(actual.VacancyApplications[0].SubmittedDate));
            Assert.That(actual.VacancyApplications[2].SubmittedDate, Is.GreaterThan(actual.VacancyApplications[1].SubmittedDate));
        }

        [Test]
        public void POST_ApplicationReviewsToUnsuccessful_RedirectsToAction()
        {
            // Arrange
            var applicationReviewIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var request = _fixture
                .Build<ApplicationReviewsToUnsuccessfulRequest>()
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.Ukprn, _ukprn)
                .With(x => x.ApplicationsToUnsuccessful, applicationReviewIds)
                .Create();

            // Act
            var actionResult = _controller.ApplicationReviewsToUnsuccessful(request, "Name", "Ascending");

            var redirectResult = actionResult as RedirectToActionResult;
            // Assert
            Assert.NotNull(actionResult);
            Assert.NotNull(redirectResult);
            Assert.AreEqual("ApplicationReviewsToUnsuccessfulFeedback", redirectResult.ActionName);
            Assert.AreEqual(_vacancyId, redirectResult.RouteValues["VacancyId"]);
            Assert.AreEqual(_ukprn, redirectResult.RouteValues["Ukprn"]);
            Assert.AreEqual(applicationReviewIds, redirectResult.RouteValues["ApplicationsToUnsuccessful"]);
        }

        [Test]
        public void GET_ApplicationReviewsToUnsuccessfulFeedback_ReturnsViewAndModelWithNoApplications()
        {
            var routeModel = _fixture.Create<ApplicationReviewsToUnsuccessfulRouteModel>();

            var result = _controller.ApplicationReviewsToUnsuccessfulFeedback(routeModel);

            var viewResult = (ViewResult)result;
            var model = viewResult.Model as ApplicationReviewsToUnsuccessfulFeedbackViewModel;

            Assert.IsNotNull(model);
            Assert.AreEqual(routeModel.Ukprn, model.Ukprn);
            Assert.AreEqual(routeModel.VacancyId, model.VacancyId);
            Assert.IsNotNull(model.ApplicationsToUnsuccessful);
        }

        [Test]
        public async Task GET_ApplicationReviewsToUnsuccessfulFeedback_ReturnsViewModelWithCorrectNumberOfApplications()
        {
            // Arrange
            var request = _fixture.Create<ShareApplicationReviewsRequest>();
            var vacancyApplication1 = _fixture.Create<VacancyApplication>();
            var vacancyApplication2 = _fixture.Create<VacancyApplication>();
            var vacancyApplications = new List<VacancyApplication> { };
            vacancyApplications.Add(vacancyApplication1);
            vacancyApplications.Add(vacancyApplication2);

            _orchestrator.Setup(o =>
                    o.GetApplicationReviewsToShareConfirmationViewModel(It.Is<ShareApplicationReviewsRequest>(y => y == request)))
                .ReturnsAsync(new ShareMultipleApplicationReviewsConfirmationViewModel
                {
                    VacancyId = request.VacancyId,
                    Ukprn = request.Ukprn,
                    ApplicationReviewsToShare = vacancyApplications
                });

            // Act
            var result = await _controller.ApplicationReviewsToShareConfirmation(request) as ViewResult;

            // Assert
            var actual = result.Model as ShareMultipleApplicationReviewsConfirmationViewModel;
            Assert.IsNotEmpty(actual.ApplicationReviewsToShare);
            Assert.That(actual.ApplicationReviewsToShare.Count(), Is.EqualTo(2));
            Assert.AreEqual(actual.Ukprn, request.Ukprn);
            Assert.AreEqual(actual.VacancyId, request.VacancyId);
        }

        [Test]
        public void POST_ApplicationReviewsToUnsuccessfulFeedback_RedirectToConfirmation()
        {
            var request = _fixture
                .Build<ApplicationReviewsToUnsuccessfulFeedbackViewModel>()
                .With(x => x.CandidateFeedback, "abc")
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.Ukprn, _ukprn)
                .Create();

            var result = _controller.ApplicationReviewsToUnsuccessfulFeedback(request) as RedirectToActionResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("ApplicationReviewsToUnsuccessfulConfirmation", result.ActionName);
            Assert.AreEqual(_ukprn, result.RouteValues["Ukprn"]);
            Assert.AreEqual(_vacancyId, result.RouteValues["VacancyId"]);
        }

        [Test]
        public async Task GET_ApplicationReviewsToUnsuccessfulConfirmation_RedirectToConfirmationView()
        {
            _orchestrator.Setup(o =>
                    o.GetApplicationReviewsToUnsuccessfulConfirmationViewModel(It.IsAny<ApplicationReviewsToUnsuccessfulRouteModel>()))
                .ReturnsAsync(new ApplicationReviewsToUnsuccessfulConfirmationViewModel { CandidateFeedback = "SomeValue" });

            var routeModel = _fixture.Create<ApplicationReviewsToUnsuccessfulRouteModel>();

            var result = await _controller.ApplicationReviewsToUnsuccessfulConfirmation(routeModel) as ViewResult;

            var actual = result.Model as ApplicationReviewsToUnsuccessfulConfirmationViewModel;

            Assert.IsNotNull(actual);
            Assert.AreEqual("SomeValue", actual.CandidateFeedback);
        }

        [Test]
        public async Task POST_ApplicationReviewsToUnsuccessfulConfirmation_MultipleApplicationsUnsuccessful_RedirectsToAction()
        {
            var applicationsToUnsuccessfulConfirmed = true;
            var vacancyApplication1 = _fixture.Create<VacancyApplication>();
            var vacancyApplication2 = _fixture.Create<VacancyApplication>();
            var vacancyApplications = new List<VacancyApplication> { };
            vacancyApplications.Add(vacancyApplication1);
            vacancyApplications.Add(vacancyApplication2);
            var request = _fixture
                .Build<ApplicationReviewsToUnsuccessfulConfirmationViewModel>()
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.Ukprn, _ukprn)
                .With(x => x.ApplicationsToUnsuccessful, vacancyApplications)
                .With(x => x.ApplicationsToUnsuccessfulConfirmed, applicationsToUnsuccessfulConfirmed)
                .Create();

            _orchestrator.Setup(o =>
                    o.PostApplicationReviewsToUnsuccessfulAsync(It.Is<ApplicationReviewsToUnsuccessfulConfirmationViewModel>(y => y == request), It.IsAny<VacancyUser>()))
                .Returns(Task.CompletedTask);

            var actionResult = await _controller.ApplicationReviewsToUnsuccessfulConfirmation(request);
            var redirectResult = actionResult as RedirectToRouteResult;

            Assert.NotNull(actionResult);
            Assert.NotNull(redirectResult);
            Assert.AreEqual(RouteNames.VacancyManage_Get, redirectResult.RouteName);
            Assert.AreEqual(_vacancyId, redirectResult.RouteValues["VacancyId"]);
            Assert.AreEqual(_ukprn, redirectResult.RouteValues["Ukprn"]);
            Assert.IsTrue(_controller.TempData.ContainsKey(TempDataKeys.ApplicationsToUnsuccessfulHeader));
            Assert.AreEqual(InfoMessages.ApplicationsToUnsuccessfulBannerHeader, _controller.TempData[TempDataKeys.ApplicationsToUnsuccessfulHeader]);
        }

        [Test]
        public async Task POST_ApplicationReviewsToUnsuccessfulConfirmation_SingleApplicationsUnsuccessful_RedirectsToAction()
        {
            var applicationsToUnsuccessfulConfirmed = true;
            var vacancyApplication1 = _fixture.Create<VacancyApplication>();
            var vacancyApplications = new List<VacancyApplication> { };
            vacancyApplications.Add(vacancyApplication1);
            var request = _fixture
                .Build<ApplicationReviewsToUnsuccessfulConfirmationViewModel>()
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.Ukprn, _ukprn)
                .With(x => x.ApplicationsToUnsuccessful, vacancyApplications)
                .With(x => x.ApplicationsToUnsuccessfulConfirmed, applicationsToUnsuccessfulConfirmed)
                .Create();

            _orchestrator.Setup(o =>
                    o.PostApplicationReviewsToUnsuccessfulAsync(It.Is<ApplicationReviewsToUnsuccessfulConfirmationViewModel>(y => y == request), It.IsAny<VacancyUser>()))
                .Returns(Task.CompletedTask);

            var actionResult = await _controller.ApplicationReviewsToUnsuccessfulConfirmation(request);
            var redirectResult = actionResult as RedirectToRouteResult;

            Assert.NotNull(actionResult);
            Assert.NotNull(redirectResult);
            Assert.AreEqual(RouteNames.VacancyManage_Get, redirectResult.RouteName);
            Assert.AreEqual(_vacancyId, redirectResult.RouteValues["VacancyId"]);
            Assert.AreEqual(_ukprn, redirectResult.RouteValues["Ukprn"]);
            Assert.IsTrue(_controller.TempData.ContainsKey(TempDataKeys.ApplicationsToUnsuccessfulHeader));
            Assert.AreEqual(string.Format(InfoMessages.ApplicationReviewUnsuccessStatusHeader, vacancyApplication1.CandidateName), _controller.TempData[TempDataKeys.ApplicationsToUnsuccessfulHeader]);
        }

        [Test]
        public async Task GET_ApplicationReviewsToShare_ReturnsViewAndModelWith3Applications()
        {
            // Arrange
            var routeModel = _fixture.Create<VacancyRouteModel>();
            var applicationReviews = CreateApplicationReviewsFixture(3);
            var sortOrder = SortOrder.Descending;
            var sortColumn = SortColumn.DateApplied;

            _orchestrator.Setup(o =>
                    o.GetApplicationReviewsToShareViewModelAsync(It.Is<VacancyRouteModel>(y => y == routeModel), It.Is<SortColumn>(x => x.Equals(sortColumn)), It.Is<SortOrder>(x => x.Equals(sortOrder))))
                .ReturnsAsync(new ShareMultipleApplicationReviewsViewModel
                {
                    VacancyId = routeModel.VacancyId,
                    Ukprn = routeModel.Ukprn,
                    VacancyApplications = applicationReviews.Sort(sortColumn, sortOrder, false).Select(c => (VacancyApplication)c).ToList()
                });

            // Act
            var result = await _controller.ApplicationReviewsToShare(routeModel, "DateApplied", "Descending") as ViewResult;

            // Assert
            var actual = result.Model as ShareMultipleApplicationReviewsViewModel;
            Assert.IsNotEmpty(actual.VacancyApplications);
            Assert.That(actual.VacancyApplications.Count(), Is.EqualTo(3));
            Assert.AreEqual(actual.Ukprn, routeModel.Ukprn);
            Assert.AreEqual(actual.VacancyId, routeModel.VacancyId);
            Assert.That(actual.VacancyApplications[0].SubmittedDate, Is.GreaterThan(actual.VacancyApplications[1].SubmittedDate));
            Assert.That(actual.VacancyApplications[1].SubmittedDate, Is.GreaterThan(actual.VacancyApplications[2].SubmittedDate));
        }

        [Test]
        public async Task GET_ApplicationReviewsToShare_ReturnsViewAndModelWithNoApplications()
        {
            // Arrange
            var routeModel = _fixture.Create<VacancyRouteModel>();

            _orchestrator.Setup(o =>
                    o.GetApplicationReviewsToShareViewModelAsync(It.Is<VacancyRouteModel>(y => y == routeModel), It.Is<SortColumn>(x => x.Equals(SortColumn.Name)), It.Is<SortOrder>(x => x.Equals(SortOrder.Ascending))))
                .ReturnsAsync(new ShareMultipleApplicationReviewsViewModel
                {
                    VacancyId = routeModel.VacancyId,
                    Ukprn = routeModel.Ukprn,
                    VacancyApplications = new List<VacancyApplication>()
                });

            // Act
            var result = await _controller.ApplicationReviewsToShare(routeModel, "Name", "Ascending") as ViewResult;

            // Assert
            var actual = result.Model as ShareMultipleApplicationReviewsViewModel;
            Assert.IsEmpty(actual.VacancyApplications);
            Assert.That(actual.VacancyApplications.Count(), Is.EqualTo(0));
            Assert.AreEqual(actual.Ukprn, routeModel.Ukprn);
            Assert.AreEqual(actual.VacancyId, routeModel.VacancyId);
        }

        [Test]
        public async Task GET_ApplicationReviewsToShare_InvalidEnumValues_DefaultSortingAppliedAscendingSubmittedDate()
        {
            // Arrange
            var routeModel = _fixture.Create<VacancyRouteModel>();
            var applicationReviews = CreateApplicationReviewsFixture(3);

            _orchestrator.Setup(o =>
                    o.GetApplicationReviewsToShareViewModelAsync(It.Is<VacancyRouteModel>(y => y == routeModel), It.Is<SortColumn>(x => x.Equals(SortColumn.Default)), It.Is<SortOrder>(x => x.Equals(SortOrder.Default))))
                .ReturnsAsync(new ShareMultipleApplicationReviewsViewModel
                {
                    VacancyId = routeModel.VacancyId,
                    Ukprn = routeModel.Ukprn,
                    VacancyApplications = applicationReviews.Sort(SortColumn.Default, SortOrder.Default, false).Select(c => (VacancyApplication)c).ToList()
                });

            // Act
            var result = await _controller.ApplicationReviewsToShare(routeModel, "InvalidSortColumn", "InvalidSortOrder") as ViewResult;

            // Assert
            var actual = result.Model as ShareMultipleApplicationReviewsViewModel;
            Assert.IsNotEmpty(actual.VacancyApplications);
            Assert.That(actual.VacancyApplications.Count(), Is.EqualTo(3));
            Assert.AreEqual(actual.Ukprn, routeModel.Ukprn);
            Assert.AreEqual(actual.VacancyId, routeModel.VacancyId);
            Assert.That(actual.VacancyApplications[1].SubmittedDate, Is.GreaterThan(actual.VacancyApplications[0].SubmittedDate));
            Assert.That(actual.VacancyApplications[2].SubmittedDate, Is.GreaterThan(actual.VacancyApplications[1].SubmittedDate));
        }

        [Test]
        public void POST_ApplicationReviewsToShare_RedirectsToAction()
        {
            // Arrange
            var listOfApplicationReviews = new List<Guid>();
            var request = _fixture
             .Build<ApplicationReviewsToShareRouteModel>()
             .With(x => x.VacancyId, _vacancyId)
             .With(x => x.Ukprn, _ukprn)
             .Create();

            // Act
            var actionResult = _controller.ApplicationReviewsToShare(request, "Name", "Ascending");
            var redirectResult = actionResult as RedirectToActionResult;

            // Assert
            Assert.NotNull(actionResult);
            Assert.NotNull(redirectResult);
            Assert.AreEqual("ApplicationReviewsToShareConfirmation", redirectResult.ActionName);
            Assert.AreEqual(_vacancyId, redirectResult.RouteValues["VacancyId"]);
            Assert.AreEqual(_ukprn, redirectResult.RouteValues["Ukprn"]);
        }

        [Test]
        public async Task GET_ApplicationReviewsToShareConfirmation_ReturnsViewModelWithCorrectNumberOfApplications()
        {
            // Arrange
            var request = _fixture.Create<ShareApplicationReviewsRequest>();
            var vacancyApplication1 = _fixture.Create<VacancyApplication>();
            var vacancyApplication2 = _fixture.Create<VacancyApplication>();
            var vacancyApplications = new List<VacancyApplication> { };
            vacancyApplications.Add(vacancyApplication1);
            vacancyApplications.Add(vacancyApplication2);

            _orchestrator.Setup(o =>
                    o.GetApplicationReviewsToShareConfirmationViewModel(It.Is<ShareApplicationReviewsRequest>(y => y == request)))
                .ReturnsAsync(new ShareMultipleApplicationReviewsConfirmationViewModel
                {
                    VacancyId = request.VacancyId,
                    Ukprn = request.Ukprn,
                    ApplicationReviewsToShare = vacancyApplications
                });

            // Act
            var result = await _controller.ApplicationReviewsToShareConfirmation(request) as ViewResult;

            // Assert
            var actual = result.Model as ShareMultipleApplicationReviewsConfirmationViewModel;
            Assert.IsNotEmpty(actual.ApplicationReviewsToShare);
            Assert.That(actual.ApplicationReviewsToShare.Count(), Is.EqualTo(2));
            Assert.AreEqual(actual.Ukprn, request.Ukprn);
            Assert.AreEqual(actual.VacancyId, request.VacancyId);
        }

        [Test]
        public async Task POST_ApplicationReviewsToShareConfirmation_MultipleApplicationsShared_RedirectsToActionWithBannerSharedMultiple()
        {
            // Arrange
            var shareApplicationsConfirmed = true;
            var vacancyApplication1 = _fixture.Create<VacancyApplication>();
            var vacancyApplication2 = _fixture.Create<VacancyApplication>();
            var vacancyApplications = new List<VacancyApplication> { };
            vacancyApplications.Add(vacancyApplication1);
            vacancyApplications.Add(vacancyApplication2);
            var request = _fixture
                .Build<ShareApplicationReviewsPostRequest>()
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.Ukprn, _ukprn)
                .With(x => x.ApplicationReviewsToShare, vacancyApplications)
                .With(x => x.ShareApplicationsConfirmed, shareApplicationsConfirmed)
                .Create();

            _orchestrator.Setup(o =>
                    o.PostApplicationReviewsToSharedAsync(It.Is<ShareApplicationReviewsPostRequest>(y => y == request), It.IsAny<VacancyUser>()))
                .Returns(Task.CompletedTask);

            // Act
            var actionResult = await _controller.ApplicationReviewsToShareConfirmation(request);
            var redirectResult = actionResult as RedirectToRouteResult;

            // Assert
            Assert.NotNull(actionResult);
            Assert.NotNull(redirectResult);
            Assert.AreEqual(RouteNames.VacancyManage_Get, redirectResult.RouteName);
            Assert.AreEqual(_vacancyId, redirectResult.RouteValues["VacancyId"]);
            Assert.AreEqual(_ukprn, redirectResult.RouteValues["Ukprn"]);
            Assert.IsTrue(_controller.TempData.ContainsKey(TempDataKeys.SharedMultipleApplicationsHeader));
            Assert.IsFalse(_controller.TempData.ContainsKey(TempDataKeys.SharedSingleApplicationsHeader));
            Assert.AreEqual(InfoMessages.SharedMultipleApplicationsBannerHeader, _controller.TempData[TempDataKeys.SharedMultipleApplicationsHeader]);
        }

        [Test]
        public async Task POST_ApplicationReviewsToShareConfirmation_SingleApplicationsShared_RedirectsToActionWithBannerSharedSingle()
        {
            // Arrange
            var shareApplicationsConfirmed = true;
            var vacancyApplication1 = _fixture.Create<VacancyApplication>();
            var vacancyApplications = new List<VacancyApplication> { };
            vacancyApplications.Add(vacancyApplication1);
            var request = _fixture
                .Build<ShareApplicationReviewsPostRequest>()
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.Ukprn, _ukprn)
                .With(x => x.ApplicationReviewsToShare, vacancyApplications)
                .With(x => x.ShareApplicationsConfirmed, shareApplicationsConfirmed)
                .Create();

            _orchestrator.Setup(o =>
                    o.PostApplicationReviewsToSharedAsync(It.Is<ShareApplicationReviewsPostRequest>(y => y == request), It.IsAny<VacancyUser>()))
                .Returns(Task.CompletedTask);

            // Act
            var actionResult = await _controller.ApplicationReviewsToShareConfirmation(request);
            var redirectResult = actionResult as RedirectToRouteResult;

            // Assert
            Assert.NotNull(actionResult);
            Assert.NotNull(redirectResult);
            Assert.AreEqual(RouteNames.VacancyManage_Get, redirectResult.RouteName);
            Assert.AreEqual(_vacancyId, redirectResult.RouteValues["VacancyId"]);
            Assert.AreEqual(_ukprn, redirectResult.RouteValues["Ukprn"]);
            Assert.IsTrue(_controller.TempData.ContainsKey(TempDataKeys.SharedSingleApplicationsHeader));
            Assert.IsFalse(_controller.TempData.ContainsKey(TempDataKeys.SharedMultipleApplicationsHeader));
            Assert.AreEqual(string.Format(InfoMessages.SharedSingleApplicationsBannerHeader, vacancyApplication1.CandidateName), _controller.TempData[TempDataKeys.SharedSingleApplicationsHeader]);
        }

        [Test]
        public async Task POST_ApplicationReviewsToShareConfirmation_NoApplicationsShared_RedirectsToActionWithNoTempDataForBanner()
        {
            // Arrange
            var shareApplicationsConfirmed = false;
            var request = _fixture
                .Build<ShareApplicationReviewsPostRequest>()
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.Ukprn, _ukprn)
                .With(x => x.ShareApplicationsConfirmed, shareApplicationsConfirmed)
                .Create();

            // Act
            var actionResult = await _controller.ApplicationReviewsToShareConfirmation(request);
            var redirectResult = actionResult as RedirectToRouteResult;

            // Assert
            Assert.NotNull(actionResult);
            Assert.NotNull(redirectResult);
            Assert.AreEqual(RouteNames.VacancyManage_Get, redirectResult.RouteName);
            Assert.AreEqual(_vacancyId, redirectResult.RouteValues["VacancyId"]);
            Assert.AreEqual(_ukprn, redirectResult.RouteValues["Ukprn"]);
            Assert.IsFalse(_controller.TempData.ContainsKey(TempDataKeys.SharedMultipleApplicationsHeader));
            Assert.IsFalse(_controller.TempData.ContainsKey(TempDataKeys.SharedSingleApplicationsHeader));
        }

        private IQueryable<ApplicationReview> CreateApplicationReviewsFixture(int numberOfApplications) 
        {
            var applications = new List<ApplicationReview> { };
            for (int i = 0; i < numberOfApplications; i++) 
            {
                var applicationReview = _fixture.Create<ApplicationReview>();
                applications.Add(applicationReview);
            }
            return applications.AsQueryable();
        }
    }
}
