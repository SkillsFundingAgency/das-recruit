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
            Assert.That(actual.VacancyApplications, Is.Not.Empty);
            Assert.That(actual.VacancyApplications.Count(), Is.EqualTo(3));
            Assert.That(actual.Ukprn, Is.EqualTo(routeModel.Ukprn));
            Assert.That(actual.VacancyId, Is.EqualTo(routeModel.VacancyId));
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
            Assert.That(actual.VacancyApplications, Is.Empty);
            Assert.That(actual.VacancyApplications.Count(), Is.EqualTo(0));
            Assert.That(actual.Ukprn, Is.EqualTo(routeModel.Ukprn));
            Assert.That(actual.VacancyId, Is.EqualTo(routeModel.VacancyId));
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
            Assert.That(actual.VacancyApplications, Is.Not.Empty);
            Assert.That(actual.VacancyApplications.Count(), Is.EqualTo(2));
            Assert.That(actual.Ukprn, Is.EqualTo(routeModel.Ukprn));
            Assert.That(actual.VacancyId, Is.EqualTo(routeModel.VacancyId));
            Assert.That(actual.ShouldMakeOthersUnsuccessfulBannerHeader, Is.EqualTo(_controller.TempData[TempDataKeys.ApplicationReviewStatusInfoMessage].ToString()));
            Assert.That(actual.ShouldMakeOthersUnsuccessfulBannerBody, Is.EqualTo(InfoMessages.ApplicationReviewSuccessStatusBannerMessage));
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
            Assert.That(actual.VacancyApplications, Is.Not.Empty);
            Assert.That(actual.VacancyApplications.Count(), Is.EqualTo(3));
            Assert.That(actual.Ukprn, Is.EqualTo(routeModel.Ukprn));
            Assert.That(actual.VacancyId, Is.EqualTo(routeModel.VacancyId));
            Assert.That(actual.VacancyApplications[1].SubmittedDate, Is.GreaterThan(actual.VacancyApplications[0].SubmittedDate));
            Assert.That(actual.VacancyApplications[2].SubmittedDate, Is.GreaterThan(actual.VacancyApplications[1].SubmittedDate));
        }

        [Test]
        public async Task POST_ApplicationReviewsToUnsuccessful_RedirectsToAction()
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
            var actionResult =await _controller.ApplicationReviewsToUnsuccessful(request, "Name", "Ascending");

            var redirectResult = actionResult as RedirectToRouteResult;
            // Assert
            Assert.That(actionResult, Is.Not.Null);
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(RouteNames.ApplicationReviewsToUnsuccessfulFeedback_Get, Is.EqualTo(redirectResult.RouteName));
            Assert.That(_vacancyId, Is.EqualTo(redirectResult.RouteValues["VacancyId"]));
            Assert.That(_ukprn, Is.EqualTo(redirectResult.RouteValues["Ukprn"]));
        }

        [Test]
        public void GET_ApplicationReviewsToUnsuccessfulFeedback_ReturnsViewAndModelWithNoApplications()
        {
            var routeModel = _fixture.Create<ApplicationReviewsToUnsuccessfulRouteModel>();

            var result = _controller.ApplicationReviewsToUnsuccessfulFeedback(routeModel);

            var viewResult = (ViewResult)result;
            var model = viewResult.Model as ApplicationReviewsToUnsuccessfulFeedbackViewModel;

            Assert.That(model, Is.Not.Null);
            Assert.That(routeModel.Ukprn, Is.EqualTo(model.Ukprn));
            Assert.That(routeModel.VacancyId, Is.EqualTo(model.VacancyId));
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
            Assert.That(actual.ApplicationReviewsToShare, Is.Not.Empty);
            Assert.That(actual.ApplicationReviewsToShare.Count(), Is.EqualTo(2));
            Assert.That(actual.Ukprn, Is.EqualTo(request.Ukprn));
            Assert.That(actual.VacancyId, Is.EqualTo(request.VacancyId));
        }

        [Test]
        public async Task POST_ApplicationReviewsToUnsuccessfulFeedback_RedirectToConfirmation()
        {
            var request = _fixture
                .Build<ApplicationReviewsToUnsuccessfulFeedbackViewModel>()
                .With(x => x.CandidateFeedback, "abc")
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.Ukprn, _ukprn)
                .Create();

            var result = await _controller.ApplicationReviewsToUnsuccessfulFeedback(request) as RedirectToRouteResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(RouteNames.ApplicationReviewsToUnsuccessfulConfirmation_Get, Is.EqualTo(result.RouteName));
            Assert.That(_ukprn, Is.EqualTo(result!.RouteValues!["Ukprn"]));
            Assert.That(_vacancyId, Is.EqualTo(result!.RouteValues!["VacancyId"]));
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

            Assert.That(actual, Is.Not.Null);
            Assert.That("SomeValue", Is.EqualTo(actual.CandidateFeedback));
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
                .With(x => x.IsMultipleApplications, true)
                .Create();

            _orchestrator.Setup(o =>
                    o.PostApplicationReviewsToUnsuccessfulAsync(It.Is<ApplicationReviewsToUnsuccessfulConfirmationViewModel>(y => y == request), It.IsAny<VacancyUser>()))
                .Returns(Task.CompletedTask);

            var actionResult = await _controller.ApplicationReviewsToUnsuccessfulConfirmation(request);
            var redirectResult = actionResult as RedirectToRouteResult;

            Assert.That(actionResult, Is.Not.Null);
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(RouteNames.VacancyManage_Get, Is.EqualTo(redirectResult.RouteName));
            Assert.That(_vacancyId, Is.EqualTo(redirectResult.RouteValues["VacancyId"]));
            Assert.That(_ukprn, Is.EqualTo(redirectResult.RouteValues["Ukprn"]));
            Assert.That(_controller.TempData.ContainsKey(TempDataKeys.ApplicationsToUnsuccessfulHeader), Is.True);
            Assert.That(InfoMessages.ApplicationsToUnsuccessfulBannerHeader, Is.EqualTo(_controller.TempData[TempDataKeys.ApplicationsToUnsuccessfulHeader]));
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
                .With(x => x.IsMultipleApplications, false)
                .Create();

            _orchestrator.Setup(o =>
                    o.PostApplicationReviewsToUnsuccessfulAsync(It.Is<ApplicationReviewsToUnsuccessfulConfirmationViewModel>(y => y == request), It.IsAny<VacancyUser>()))
                .Returns(Task.CompletedTask);

            var actionResult = await _controller.ApplicationReviewsToUnsuccessfulConfirmation(request);
            var redirectResult = actionResult as RedirectToRouteResult;

            Assert.That(actionResult, Is.Not.Null);
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(RouteNames.VacancyManage_Get, Is.EqualTo(redirectResult.RouteName));
            Assert.That(_vacancyId, Is.EqualTo(redirectResult.RouteValues["VacancyId"]));
            Assert.That(_ukprn, Is.EqualTo(redirectResult.RouteValues["Ukprn"]));
            Assert.That(_controller.TempData.ContainsKey(TempDataKeys.ApplicationsToUnsuccessfulHeader), Is.True);
            Assert.That(string.Format(InfoMessages.ApplicationEmployerUnsuccessfulHeader), Is.EqualTo(_controller.TempData[TempDataKeys.ApplicationsToUnsuccessfulHeader]));
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
            Assert.That(actual.VacancyApplications, Is.Not.Empty);
            Assert.That(actual.VacancyApplications.Count(), Is.EqualTo(3));
            Assert.That(actual.Ukprn, Is.EqualTo(routeModel.Ukprn));
            Assert.That(actual.VacancyId, Is.EqualTo(routeModel.VacancyId));
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
            Assert.That(actual.VacancyApplications, Is.Empty);
            Assert.That(actual.VacancyApplications.Count(), Is.EqualTo(0));
            Assert.That(actual.Ukprn, Is.EqualTo(routeModel.Ukprn));
            Assert.That(actual.VacancyId, Is.EqualTo(routeModel.VacancyId));
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
            Assert.That(actual.VacancyApplications, Is.Not.Empty);
            Assert.That(actual.VacancyApplications.Count(), Is.EqualTo(3));
            Assert.That(actual.Ukprn, Is.EqualTo(routeModel.Ukprn));
            Assert.That(actual.VacancyId, Is.EqualTo(routeModel.VacancyId));
            Assert.That(actual.VacancyApplications[1].SubmittedDate, Is.GreaterThan(actual.VacancyApplications[0].SubmittedDate));
            Assert.That(actual.VacancyApplications[2].SubmittedDate, Is.GreaterThan(actual.VacancyApplications[1].SubmittedDate));
        }

        [Test]
        public async Task POST_ApplicationReviewsToShare_RedirectsToAction()
        {
            // Arrange
            var request = _fixture
             .Build<ApplicationReviewsToShareRouteModel>()
             .With(x => x.VacancyId, _vacancyId)
             .With(x => x.Ukprn, _ukprn)
             .Create();

            // Act
            var actionResult = await _controller.ApplicationReviewsToShare(request, "Name", "Ascending");
            var redirectResult = actionResult as RedirectToRouteResult;

            // Assert
            Assert.That(actionResult, Is.Not.Null);
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(RouteNames.ApplicationReviewsToShareConfirmation_Get, Is.EqualTo(redirectResult.RouteName));
            Assert.That(_vacancyId, Is.EqualTo(redirectResult.RouteValues["VacancyId"]));
            Assert.That(_ukprn, Is.EqualTo(redirectResult.RouteValues["Ukprn"]));
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
            Assert.That(actual.ApplicationReviewsToShare, Is.Not.Empty);
            Assert.That(actual.ApplicationReviewsToShare.Count(), Is.EqualTo(2));
            Assert.That(actual.Ukprn, Is.EqualTo(request.Ukprn));
            Assert.That(actual.VacancyId, Is.EqualTo(request.VacancyId));
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
                .With(x=>x.SharingMultipleApplications, true)
                .Create();

           
            // Act
            var actionResult = await _controller.ApplicationReviewsToShareConfirmation(request);
            var redirectResult = actionResult as RedirectToRouteResult;

            // Assert
            Assert.That(actionResult, Is.Not.Null);
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(RouteNames.VacancyManage_Get, Is.EqualTo(redirectResult.RouteName));
            Assert.That(_vacancyId, Is.EqualTo(redirectResult.RouteValues["VacancyId"]));
            Assert.That(_ukprn, Is.EqualTo(redirectResult.RouteValues["Ukprn"]));
            Assert.That(_controller.TempData.ContainsKey(TempDataKeys.SharedMultipleApplicationsHeader), Is.True);
            Assert.That(_controller.TempData.ContainsKey(TempDataKeys.SharedSingleApplicationsHeader), Is.False);
            Assert.That(InfoMessages.SharedMultipleApplicationsBannerHeader, Is.EqualTo(_controller.TempData[TempDataKeys.SharedMultipleApplicationsHeader]));
            _orchestrator.Verify(o =>
                o.PostApplicationReviewsStatus(It.Is<ApplicationReviewsToUpdateStatusModel>(y => y.VacancyId == request.VacancyId), It.IsAny<VacancyUser>(), ApplicationReviewStatus.Shared, null));

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
                .With(x => x.SharingMultipleApplications, false)
                .Create();

            

            // Act
            var actionResult = await _controller.ApplicationReviewsToShareConfirmation(request);
            var redirectResult = actionResult as RedirectToRouteResult;

            // Assert
            Assert.That(actionResult, Is.Not.Null);
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(RouteNames.VacancyManage_Get, Is.EqualTo(redirectResult.RouteName));
            Assert.That(_vacancyId, Is.EqualTo(redirectResult.RouteValues["VacancyId"]));
            Assert.That(_ukprn, Is.EqualTo(redirectResult.RouteValues["Ukprn"]));
            Assert.That(_controller.TempData.ContainsKey(TempDataKeys.SharedSingleApplicationsHeader), Is.True);
            Assert.That(_controller.TempData.ContainsKey(TempDataKeys.SharedMultipleApplicationsHeader), Is.False);
            Assert.That(string.Format(InfoMessages.SharedSingleApplicationsBannerHeader, vacancyApplication1.CandidateName), Is.EqualTo(_controller.TempData[TempDataKeys.SharedSingleApplicationsHeader]));
            _orchestrator.Verify(o =>
                o.PostApplicationReviewsStatus(It.Is<ApplicationReviewsToUpdateStatusModel>(y => y.VacancyId == request.VacancyId), It.IsAny<VacancyUser>(), ApplicationReviewStatus.Shared, null));
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
            Assert.That(actionResult, Is.Not.Null);
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(RouteNames.VacancyManage_Get, Is.EqualTo(redirectResult.RouteName));
            Assert.That(_vacancyId, Is.EqualTo(redirectResult.RouteValues["VacancyId"]));
            Assert.That(_ukprn, Is.EqualTo(redirectResult.RouteValues["Ukprn"]));
            Assert.That(_controller.TempData.ContainsKey(TempDataKeys.SharedMultipleApplicationsHeader), Is.False);
            Assert.That(_controller.TempData.ContainsKey(TempDataKeys.SharedSingleApplicationsHeader), Is.False);
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
