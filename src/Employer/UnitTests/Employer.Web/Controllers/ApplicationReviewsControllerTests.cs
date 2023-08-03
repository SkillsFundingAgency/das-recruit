using System.Threading.Tasks;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Esfa.Recruit.Employer.Web.Controllers;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.ViewModels.ApplicationReviews;

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

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _orchestrator = new Mock<IApplicationReviewsOrchestrator>();
            _vacancyId = Guid.NewGuid();
            _applicationReviewId = Guid.NewGuid();
            _employerAccountId = "ADGFHAS";
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
        public async Task GET_ApplicationReviewsToUnsuccessful_ReturnsViewAndModelWith2Applications()
        {
            // Arrange
            var routeModel = _fixture.Create<VacancyRouteModel>();
            var vacancyApplication1 = _fixture.Create<VacancyApplication>();
            var vacancyApplication2 = _fixture.Create<VacancyApplication>();
            var vacancyApplications = new List<VacancyApplication> { };
            vacancyApplications.Add(vacancyApplication1);
            vacancyApplications.Add(vacancyApplication2);

            _orchestrator.Setup(o =>
                    o.GetApplicationReviewsToUnsuccessfulViewModelAsync(It.Is<VacancyRouteModel>(y => y == routeModel)))
                .ReturnsAsync(new ApplicationReviewsToUnsuccessfulViewModel
                {
                    VacancyId = routeModel.VacancyId,
                    EmployerAccountId = routeModel.EmployerAccountId,
                    VacancyApplications = vacancyApplications
                });

            // Act
            var result = await _controller.ApplicationReviewsToUnsuccessful(routeModel) as ViewResult;

            // Assert
            var actual = result.Model as ApplicationReviewsToUnsuccessfulViewModel;
            Assert.IsNotEmpty(actual.VacancyApplications);
            Assert.That(actual.VacancyApplications.Count(), Is.EqualTo(2));
            Assert.AreEqual(actual.VacancyId, routeModel.VacancyId);
            Assert.AreEqual(actual.EmployerAccountId, routeModel.EmployerAccountId);
        }

        [Test]
        public async Task GET_ApplicationReviewsToUnsuccessful_ReturnsViewAndModelWithNoApplications()
        {
            // Arrange
            var routeModel = _fixture.Create<VacancyRouteModel>();

            _orchestrator.Setup(o =>
                    o.GetApplicationReviewsToUnsuccessfulViewModelAsync(It.Is<VacancyRouteModel>(y => y == routeModel)))
                .ReturnsAsync(new ApplicationReviewsToUnsuccessfulViewModel
                {
                    VacancyId = routeModel.VacancyId,
                    EmployerAccountId = routeModel.EmployerAccountId,
                    VacancyApplications = new List<VacancyApplication>()
                });

            // Act
            var result = await _controller.ApplicationReviewsToUnsuccessful(routeModel) as ViewResult;

            // Assert
            var actual = result.Model as ApplicationReviewsToUnsuccessfulViewModel;
            Assert.IsEmpty(actual.VacancyApplications);
            Assert.That(actual.VacancyApplications.Count(), Is.EqualTo(0));
            Assert.AreEqual(actual.VacancyId, routeModel.VacancyId);
            Assert.AreEqual(actual.EmployerAccountId, routeModel.EmployerAccountId);
        }

        [Test]
        public async Task POST_ApplicationReviewsToUnsuccessful_RedirectsToAction()
        {
            // Arrange
            var listOfApplicationReviews = new List<Guid>();
            listOfApplicationReviews.Add(_applicationReviewId);
            var request = _fixture
                .Build<ApplicationReviewsToUnsuccessfulRouteModel>()
                .With(x => x.VacancyId, _vacancyId)
                .With(x => x.EmployerAccountId, _employerAccountId)
                .With(x => x.ApplicationsToUnsuccessful, listOfApplicationReviews)
                .Create();

            // Act
            var redirectResult = await _controller.ApplicationReviewsToUnsuccessfulAsync(request) as RedirectToActionResult;

            // Assert
            Assert.NotNull(redirectResult);
            Assert.AreEqual("ApplicationReviewsToUnsuccessfulConfirmation", redirectResult.ActionName);
            Assert.AreEqual(_vacancyId, redirectResult.RouteValues["VacancyId"]);
            Assert.AreEqual(_employerAccountId, redirectResult.RouteValues["EmployerAccountId"]);
        }
    }
}