﻿using System.Threading.Tasks;
using AutoFixture;
using Esfa.Recruit.Employer.Web;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.ViewModels.ApplicationReview;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Moq;
using NUnit.Framework;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Orchestrators
{
    public class ApplicationReviewOrchestratorTests
    {
        private Fixture _fixture;
        private Mock<IEmployerVacancyClient> _employerVacancyClient;
        private Mock<IUtility> _utility;
        private ApplicationReviewOrchestrator _orchestrator;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _employerVacancyClient = new Mock<IEmployerVacancyClient>();
            _utility = new Mock<IUtility>();
            _orchestrator = new ApplicationReviewOrchestrator(_employerVacancyClient.Object, _utility.Object);
        }

        [Test]
        public async Task PostApplicationReviewEditModelAsyn_ReturnsCandidateInfo()
        {
            var model = _fixture.Create<ApplicationReviewEditModel>();
            var vacancyUser = _fixture.Create<VacancyUser>();

            var applicationReview = _fixture.Create<ApplicationReview>();

            _utility.Setup(x => x.GetAuthorisedApplicationReviewAsync(model, false))
                .ReturnsAsync(applicationReview);
            _employerVacancyClient.Setup(x => x.SetApplicationReviewStatus(model.ApplicationReviewId, model.Outcome, model.CandidateFeedback, vacancyUser))
                .ReturnsAsync(false);

            var result = await _orchestrator.PostApplicationReviewEditModelAsync(model, vacancyUser);

            Assert.That(applicationReview.Id, Is.EqualTo(result.ApplicationReviewId));
            Assert.That(applicationReview.GetFriendlyId(), Is.EqualTo(result.FriendlyId));
            Assert.That(applicationReview.Application.FullName, Is.EqualTo(result.Name));
        }
    }
}
