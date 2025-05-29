using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.NUnit3;
using Esfa.Recruit.Provider.Web.Controllers;
using Esfa.Recruit.Provider.Web.Models.ApplicationReviews;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.ApplicationReviews;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Extensions;
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
            var applicationReview1 = _fixture.Create<ApplicationReview>();
            var applicationReview2 = _fixture.Create<ApplicationReview>();
            var applicationReview3 = _fixture.Create<ApplicationReview>();
            var applications = new List<ApplicationReview>
            {
                applicationReview1,
                applicationReview2,
                applicationReview3
            };
            foreach (var applicationReview in applications)
            {
                applicationReview.IsWithdrawn = false;
            }
            const SortColumn sortColumn = SortColumn.DateApplied;
            const SortOrder sortOrder = SortOrder.Descending;

            _vacancyClient.Setup(x => x.GetVacancyAsync(routeModel.VacancyId.GetValueOrDefault()))
                .ReturnsAsync(vacancy);
            _vacancyClient.Setup(x => x.GetVacancyApplicationsSortedAsync(vacancy.VacancyReference.Value, It.Is<SortColumn>(x => x.Equals(sortColumn)), It.Is<SortOrder>(x => x.Equals(sortOrder)), false))
                .ReturnsAsync(applications.AsQueryable().Sort(sortColumn, sortOrder, false).Select(c => (VacancyApplication)c).ToList());
            _vacancyClient
                .Setup(x => x.GetVacancyApplicationsForReferenceAndStatus(routeModel.VacancyId.Value,
                    ApplicationReviewStatus.PendingToMakeUnsuccessful)).ReturnsAsync(new List<VacancyApplication>());

            // Act
            var viewModel = await _orchestrator.GetApplicationReviewsToUnsuccessfulViewModelAsync(routeModel, sortColumn, sortOrder);

            // Assert
            Assert.That(viewModel.VacancyApplications, Is.Not.Empty);
            Assert.That(viewModel.VacancyApplications.Count, Is.EqualTo(applications.Count));
            Assert.That(viewModel.Ukprn, Is.EqualTo(routeModel.Ukprn));
            Assert.That(viewModel.VacancyId, Is.EqualTo(vacancy.Id));
            Assert.That(viewModel.VacancyApplications[0].SubmittedDate, Is.GreaterThan(viewModel.VacancyApplications[1].SubmittedDate));
            Assert.That(viewModel.VacancyApplications[1].SubmittedDate, Is.GreaterThan(viewModel.VacancyApplications[2].SubmittedDate));
        }

        [Test, MoqAutoData]
        public async Task GetApplicationReviewsToUnsuccessfulConfirmationViewModel_ReturnsViewModelWithCorrectData(
            List<VacancyApplication> vacancyApplications,
            ApplicationReviewsToUnsuccessfulRouteModel request,
            [Frozen] Mock<IRecruitVacancyClient> vacancyClient,
            ApplicationReviewsOrchestrator orchestrator)
        {
            vacancyClient.Setup(x => x.GetVacancyApplicationsForReferenceAndStatus(request.VacancyId.Value,
                    ApplicationReviewStatus.PendingToMakeUnsuccessful))
                .ReturnsAsync(vacancyApplications);

            var viewModel = await orchestrator.GetApplicationReviewsToUnsuccessfulConfirmationViewModel(request);

            viewModel.ApplicationsToUnsuccessful.Should().BeEquivalentTo(vacancyApplications);
            viewModel.CandidateFeedback.Should().Be(vacancyApplications.First().CandidateFeedback);
            Assert.That(viewModel.ApplicationsToUnsuccessful, Is.Not.Empty);
            Assert.That(viewModel.ApplicationsToUnsuccessful.Count(), Is.EqualTo(vacancyApplications.Count));
            Assert.That(viewModel.Ukprn, Is.EqualTo(request.Ukprn));
            Assert.That(viewModel.VacancyId, Is.EqualTo(request.VacancyId));
        }

        [Test]
        public async Task GetApplicationReviewsToShareViewModelAsync_ReturnsViewModelWithCorrectData()
        {
            // Arrange
            var routeModel = _fixture.Create<VacancyRouteModel>();
            var vacancy = _fixture.Create<Vacancy>();
            var vacancyApplication1 = _fixture.Create<VacancyApplication>();
            var vacancyApplication2 = _fixture.Create<VacancyApplication>();
            var vacancyApplications = new List<VacancyApplication>
            {
                vacancyApplication1,
                vacancyApplication2
            };
            foreach (var vacancyApplication in vacancyApplications)
            {
                vacancyApplication.IsWithdrawn = false;
            }

            _vacancyClient.Setup(x => x.GetVacancyAsync(routeModel.VacancyId.GetValueOrDefault()))
                .ReturnsAsync(vacancy);
            _vacancyClient.Setup(x => x.GetVacancyApplicationsSortedAsync(vacancy.VacancyReference.Value, It.IsAny<SortColumn>(), It.IsAny<SortOrder>(), false))
                .ReturnsAsync(vacancyApplications);
            _vacancyClient
                .Setup(x => x.GetVacancyApplicationsForReferenceAndStatus(routeModel.VacancyId.Value,
                    ApplicationReviewStatus.PendingShared)).ReturnsAsync(vacancyApplications);

            // Act
            var viewModel = await _orchestrator.GetApplicationReviewsToShareViewModelAsync(routeModel, SortColumn.Name, SortOrder.Ascending);

            // Assert
            Assert.That(viewModel.VacancyApplications, Is.Not.Empty);
            Assert.That(viewModel.VacancyApplications.Count, Is.EqualTo(vacancyApplications.Count));
            Assert.That(viewModel.Ukprn, Is.EqualTo(routeModel.Ukprn));
            Assert.That(viewModel.VacancyId, Is.EqualTo(vacancy.Id));
            Assert.That(viewModel.VacancyReference, Is.EqualTo(vacancy.VacancyReference));
        }

        [Test]
        public async Task GetApplicationReviewsToShareViewModelAsync_With_WithDrawn_Status_ReturnsViewModelWithCorrectData()
        {
            // Arrange
            var routeModel = _fixture.Create<VacancyRouteModel>();
            var vacancy = _fixture.Create<Vacancy>();
            var vacancyApplication1 = _fixture.Create<VacancyApplication>();
            var vacancyApplication2 = _fixture.Create<VacancyApplication>();
            var vacancyApplications = new List<VacancyApplication>
            {
                vacancyApplication1,
                vacancyApplication2
            };
            foreach (var vacancyApplication in vacancyApplications)
            {
                vacancyApplication.IsWithdrawn = true;
            }

            _vacancyClient.Setup(x => x.GetVacancyAsync(routeModel.VacancyId.GetValueOrDefault()))
                .ReturnsAsync(vacancy);
            _vacancyClient.Setup(x => x.GetVacancyApplicationsSortedAsync(vacancy.VacancyReference.Value, It.IsAny<SortColumn>(), It.IsAny<SortOrder>(), false))
                .ReturnsAsync(vacancyApplications);
            _vacancyClient
                .Setup(x => x.GetVacancyApplicationsForReferenceAndStatus(routeModel.VacancyId.Value,
                    ApplicationReviewStatus.PendingShared)).ReturnsAsync(vacancyApplications);

            // Act
            var viewModel = await _orchestrator.GetApplicationReviewsToShareViewModelAsync(routeModel, SortColumn.Name, SortOrder.Ascending);

            // Assert
            Assert.That(viewModel.VacancyApplications.Count, Is.EqualTo(0));
            Assert.That(viewModel.Ukprn, Is.EqualTo(routeModel.Ukprn));
            Assert.That(viewModel.VacancyId, Is.EqualTo(vacancy.Id));
            Assert.That(viewModel.VacancyReference, Is.EqualTo(vacancy.VacancyReference));
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

            _vacancyClient.Setup(x => x.GetVacancyApplicationsForReferenceAndStatus(request.VacancyId!.Value,ApplicationReviewStatus.PendingShared))
                .ReturnsAsync(vacancyApplications);

            // Act
            var viewModel = await _orchestrator.GetApplicationReviewsToShareConfirmationViewModel(request);

            // Assert
            Assert.That(viewModel.ApplicationReviewsToShare, Is.Not.Empty);
            Assert.That(viewModel.ApplicationReviewsToShare.Count(), Is.EqualTo(vacancyApplications.Count()));
            Assert.That(viewModel.Ukprn, Is.EqualTo(request.Ukprn));
            Assert.That(viewModel.VacancyId, Is.EqualTo(request.VacancyId));
        }
    }
}
