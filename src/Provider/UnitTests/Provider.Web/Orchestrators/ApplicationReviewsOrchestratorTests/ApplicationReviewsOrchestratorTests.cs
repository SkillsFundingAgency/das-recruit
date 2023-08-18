﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Esfa.Recruit.Provider.Web.Controllers;
using Esfa.Recruit.Provider.Web.Models.ApplicationReviews;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.ApplicationReviews;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Orchestrators.ApplicationReviewsOrchestratorTests
{
    public class ApplicationReviewsOrchestratorTests
    {
        private Fixture _fixture;
        private Mock<IRecruitVacancyClient> _vacancyClient;
        private ApplicationReviewsOrchestrator _orchestrator;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _vacancyClient = new Mock<IRecruitVacancyClient>();
            _orchestrator = new ApplicationReviewsOrchestrator(_vacancyClient.Object);
        }

        [Test]
        public async Task GetApplicationReviewsToUnsuccessfulViewModelAsync_ReturnsViewModelWithCorrectData()
        {
            // Arrange
            var routeModel = _fixture.Create<VacancyRouteModel>();
            var vacancy = _fixture.Create<Vacancy>();
            var vacancyApplication1 = _fixture.Create<VacancyApplication>();
            var vacancyApplication2 = _fixture.Create<VacancyApplication>();
            var vacancyApplications = new List<VacancyApplication> { };
            vacancyApplications.Add(vacancyApplication1);
            vacancyApplications.Add(vacancyApplication2);

            _vacancyClient.Setup(x => x.GetVacancyAsync(routeModel.VacancyId.GetValueOrDefault()))
                .ReturnsAsync(vacancy);
            _vacancyClient.Setup(x => x.GetVacancyApplicationsSortedAsync(vacancy.VacancyReference.Value, It.IsAny<SortColumn>(), It.IsAny<SortOrder>(), false))
                .ReturnsAsync(vacancyApplications);

            // Act
            var viewModel = await _orchestrator.GetApplicationReviewsToUnsuccessfulViewModelAsync(routeModel, SortColumn.Name, SortOrder.Descending);

            // Assert
            Assert.IsNotEmpty(viewModel.VacancyApplications);
            Assert.That(viewModel.VacancyApplications.Count(), Is.EqualTo(vacancyApplications.Count()));
            Assert.AreEqual(viewModel.Ukprn, routeModel.Ukprn);
            Assert.AreEqual(viewModel.VacancyId, vacancy.Id);
        }

        [Test]
        public async Task GetApplicationReviewsToUnsuccessfulConfirmationViewModel_ReturnsViewModelWithCorrectData()
        {
            var request = _fixture.Create<ApplicationReviewsToUnsuccessfulRouteModel>();
            var vacancyApplication1 = _fixture.Create<VacancyApplication>();
            var vacancyApplication2 = _fixture.Create<VacancyApplication>();
            var vacancyApplications = new List<VacancyApplication> { };
            vacancyApplications.Add(vacancyApplication1);
            vacancyApplications.Add(vacancyApplication2);

            _vacancyClient.Setup(x => x.GetVacancyApplicationsForSelectedIdsAsync(request.ApplicationsToUnsuccessful))
                .ReturnsAsync(vacancyApplications);

            var viewModel = await _orchestrator.GetApplicationReviewsToUnsuccessfulConfirmationViewModel(request);

            Assert.IsNotEmpty(viewModel.ApplicationsToUnsuccessful);
            Assert.That(viewModel.ApplicationsToUnsuccessful.Count(), Is.EqualTo(vacancyApplications.Count()));
            Assert.AreEqual(viewModel.Ukprn, request.Ukprn);
            Assert.AreEqual(viewModel.VacancyId, request.VacancyId);
        }

        [Test]
        public async Task GetApplicationReviewsToShareViewModelAsync_ReturnsViewModelWithCorrectData()
        {
            // Arrange
            var routeModel = _fixture.Create<VacancyRouteModel>();
            var vacancy = _fixture.Create<Vacancy>();
            var vacancyApplication1 = _fixture.Create<VacancyApplication>();
            var vacancyApplication2 = _fixture.Create<VacancyApplication>();
            var vacancyApplications = new List<VacancyApplication> { };
            vacancyApplications.Add(vacancyApplication1);
            vacancyApplications.Add(vacancyApplication2);

            _vacancyClient.Setup(x => x.GetVacancyAsync(routeModel.VacancyId.GetValueOrDefault()))
                .ReturnsAsync(vacancy);
            _vacancyClient.Setup(x => x.GetVacancyApplicationsSortedAsync(vacancy.VacancyReference.Value, It.IsAny<SortColumn>(), It.IsAny<SortOrder>(), false))
                .ReturnsAsync(vacancyApplications);

            // Act
            var viewModel = await _orchestrator.GetApplicationReviewsToShareViewModelAsync(routeModel, SortColumn.Name, SortOrder.Ascending);

            // Assert
            Assert.IsNotEmpty(viewModel.VacancyApplications);
            Assert.That(viewModel.VacancyApplications.Count(), Is.EqualTo(vacancyApplications.Count()));
            Assert.AreEqual(viewModel.Ukprn, routeModel.Ukprn);
            Assert.AreEqual(viewModel.VacancyId, vacancy.Id);
            Assert.AreEqual(viewModel.VacancyReference, vacancy.VacancyReference);
        }

        [Test]
        public async Task GetApplicationReviewsToShareConfirmationViewModel_ReturnsViewModelWithCorrectData()
        {
            // Arrange
            var request = _fixture.Create<ShareApplicationReviewsRequest>();
            var vacancyApplication1 = _fixture.Create<VacancyApplication>();
            var vacancyApplication2 = _fixture.Create<VacancyApplication>();
            var vacancyApplications = new List<VacancyApplication> { };
            vacancyApplications.Add(vacancyApplication1);
            vacancyApplications.Add(vacancyApplication2);

            _vacancyClient.Setup(x => x.GetVacancyApplicationsForSelectedIdsAsync(request.ApplicationsToShare))
                .ReturnsAsync(vacancyApplications);

            // Act
            var viewModel = await _orchestrator.GetApplicationReviewsToShareConfirmationViewModel(request);

            // Assert
            Assert.IsNotEmpty(viewModel.ApplicationReviewsToShare);
            Assert.That(viewModel.ApplicationReviewsToShare.Count(), Is.EqualTo(vacancyApplications.Count()));
            Assert.AreEqual(viewModel.Ukprn, request.Ukprn);
            Assert.AreEqual(viewModel.VacancyId, request.VacancyId);
        }
    }
}
