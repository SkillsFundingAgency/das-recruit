﻿using System.Threading.Tasks;
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
        public async Task GET_ApplicationReviews_ReturnsViewAndModelWith2Applications()
        {
            // Arrange
            var routeModel = _fixture.Create<VacancyRouteModel>();
            var vacancyApplication1 = _fixture.Create<VacancyApplication>();
            var vacancyApplication2 = _fixture.Create<VacancyApplication>();
            var vacancyApplications = new List<VacancyApplication> { };
            vacancyApplications.Add(vacancyApplication1);
            vacancyApplications.Add(vacancyApplication2);

            _orchestrator.Setup(o =>
                    o.GetApplicationReviewsToShareViewModelAsync(It.Is<VacancyRouteModel>(y => y == routeModel)))
                .ReturnsAsync(new ShareMultipleApplicationReviewsViewModel
                {
                    VacancyId = routeModel.VacancyId,
                    Ukprn = routeModel.Ukprn,
                    VacancyApplications = vacancyApplications
                });

            // Act
            var result = await _controller.ApplicationReviews(routeModel) as ViewResult;

            // Assert
            var actual = result.Model as ShareMultipleApplicationReviewsViewModel;
            Assert.IsNotEmpty(actual.VacancyApplications);
            Assert.That(actual.VacancyApplications.Count(), Is.EqualTo(2));
            Assert.AreEqual(actual.Ukprn, routeModel.Ukprn);
            Assert.AreEqual(actual.VacancyId, routeModel.VacancyId);
        }

        [Test]
        public async Task GET_ApplicationReviews_ReturnsViewAndModelWithNoApplications()
        {
            // Arrange
            var routeModel = _fixture.Create<VacancyRouteModel>();

            _orchestrator.Setup(o =>
                    o.GetApplicationReviewsToShareViewModelAsync(It.Is<VacancyRouteModel>(y => y == routeModel)))
                .ReturnsAsync(new ShareMultipleApplicationReviewsViewModel
                {
                    VacancyId = routeModel.VacancyId,
                    Ukprn = routeModel.Ukprn,
                    VacancyApplications = new List<VacancyApplication>()
                });

            // Act
            var result = await _controller.ApplicationReviews(routeModel) as ViewResult;

            // Assert
            var actual = result.Model as ShareMultipleApplicationReviewsViewModel;
            Assert.IsEmpty(actual.VacancyApplications);
            Assert.That(actual.VacancyApplications.Count(), Is.EqualTo(0));
            Assert.AreEqual(actual.Ukprn, routeModel.Ukprn);
            Assert.AreEqual(actual.VacancyId, routeModel.VacancyId);
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
            var actionResult = _controller.ApplicationReviewsToShare(request);
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
                .With(x =>x.ShareApplicationsConfirmed, shareApplicationsConfirmed)
                .Create();

            _orchestrator.Setup(o =>
                    o.PostApplicationReviewsStatusConfirmationAsync(It.Is<ShareApplicationReviewsPostRequest>(y => y == request), It.IsAny<VacancyUser>()))
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
                    o.PostApplicationReviewsStatusConfirmationAsync(It.Is<ShareApplicationReviewsPostRequest>(y => y == request), It.IsAny<VacancyUser>()))
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
    }
}