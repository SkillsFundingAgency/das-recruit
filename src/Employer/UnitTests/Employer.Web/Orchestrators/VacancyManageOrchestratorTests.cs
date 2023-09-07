using AutoFixture;
using Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Orchestrators
{
    public class VacancyManageOrchestratorTests
    {
        private Fixture _fixture;
        private VacancyManageOrchestrator _orchestrator;
        private Mock<IRecruitVacancyClient> _vacancyClient;
        private Mock<IUtility> _utility;
        private Mock<EmployerRecruitSystemConfiguration> _systemConfig;
        private Mock<ILogger<VacancyManageOrchestrator>> _logger;
        private Mock<IGeocodeImageService> _geocodeImageService;
        private Mock<IOptions<ExternalLinksConfiguration>> _externalLinksConfig;
        private DisplayVacancyViewModelMapper _vacancyDisplayMapper;
        private Guid _vacancyId;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _vacancyClient = new Mock<IRecruitVacancyClient>();
            _utility = new Mock<IUtility>();
            _systemConfig = new Mock<EmployerRecruitSystemConfiguration>();
            _logger = new Mock<ILogger<VacancyManageOrchestrator>>();
            _geocodeImageService = new Mock<IGeocodeImageService>();
            _externalLinksConfig = new Mock<IOptions<ExternalLinksConfiguration>>();
            _vacancyDisplayMapper = new DisplayVacancyViewModelMapper(_geocodeImageService.Object, _externalLinksConfig.Object, _vacancyClient.Object);
            _orchestrator = new VacancyManageOrchestrator(_logger.Object, _vacancyDisplayMapper, _vacancyClient.Object, _systemConfig.Object, _utility.Object);
            _vacancyId = Guid.NewGuid();
        }

        [Test]
        public async Task GetManageVacancyViewModel_ReturnsCorrectViewModel()
        {
            // Arrange
            var vacancy = _fixture.Build<Vacancy>()
                .With(x => x.Id, _vacancyId)
                .Create();
            var sortOrder = SortOrder.Descending;
            var sortColumn = SortColumn.Status;
            var vacancyApplications = new List<VacancyApplication> { };
            var vacancyApplication1 = _fixture.Build<VacancyApplication>()
                .With(x => x.HasEverBeenEmployerInterviewing, true)
                .With(x => x.FirstName, "Jack")
                .With(x => x.LastName, "Sparrow")
                .Create();
            var vacancyApplication2 = _fixture.Build<VacancyApplication>()
                .With(x => x.HasEverBeenEmployerInterviewing, false)
                .With(x => x.FirstName, "Jamie")
                .With(x => x.LastName, "Vardy")
                .Create();
            vacancyApplications.Add(vacancyApplication1);
            vacancyApplications.Add(vacancyApplication2);
            var vacancyShared = false;

            _vacancyClient.Setup(x => x.GetVacancyApplicationsSortedAsync((long)vacancy.VacancyReference, sortColumn, sortOrder, vacancyShared))
                .ReturnsAsync(vacancyApplications);

            // Act
            var viewModel = await _orchestrator.GetManageVacancyViewModel(vacancy, sortColumn, sortOrder, vacancyShared);

            // Assert
            Assert.AreEqual(_vacancyId, viewModel.VacancyId);
            Assert.AreEqual(vacancy.EmployerAccountId.ToUpperInvariant(), viewModel.EmployerAccountId);
            Assert.AreEqual(vacancy.Title, viewModel.Title);
            Assert.AreEqual(vacancy.Status, viewModel.Status);
            Assert.AreEqual(vacancy.CanExtendStartAndClosingDates, viewModel.CanShowEditVacancyLink);
            Assert.AreEqual(vacancy.IsDisabilityConfident, viewModel.IsDisabilityConfident);
            Assert.AreEqual(vacancyApplications.Count, viewModel.Applications.Applications.Count());
            Assert.AreEqual(vacancyApplications[0].CandidateName, viewModel.Applications.Applications.First().CandidateName);
            Assert.IsTrue(viewModel.Applications.Applications.First().ShowCandidateName);
            Assert.IsFalse(viewModel.Applications.Applications.First().ShowApplicantID);
            Assert.AreEqual(vacancyApplications[1].CandidateName, viewModel.Applications.Applications.ElementAt(1).CandidateName);
            Assert.IsFalse(viewModel.Applications.Applications.ElementAt(1).ShowCandidateName);
            Assert.IsTrue(viewModel.Applications.Applications.ElementAt(1).ShowApplicantID);

        }

    }
}
