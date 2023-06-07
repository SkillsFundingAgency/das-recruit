using System.Threading.Tasks;
using System.Threading;
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
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using System;

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
            _controller = new ApplicationReviewsController(_orchestrator.Object);
            _vacancyId = Guid.NewGuid();
            _ukprn = 10000234;
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
                    o.GetApplicationReviewsToShareWithEmployerViewModelAsync(It.Is<VacancyRouteModel>(y => y == routeModel)))
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
                    o.GetApplicationReviewsToShareWithEmployerViewModelAsync(It.Is<VacancyRouteModel>(y => y == routeModel)))
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
    }
}
