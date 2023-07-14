using System.Threading.Tasks;
using AutoFixture;
using Esfa.Recruit.Provider.Web;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.ApplicationReview;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Moq;
using NUnit.Framework;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Orchestrators.ApplicationReviewOrchestratorTests
{

    public class ApplicationReviewOrchestratorTests
    {
        private Fixture _fixture;
        private Mock<IRecruitVacancyClient> _recruitVacancyClient;
        private Mock<IEmployerVacancyClient> _employerVacancyClient;
        private Mock<IUtility> _utility;
        private ApplicationReviewOrchestrator _orchestrator;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _recruitVacancyClient = new Mock<IRecruitVacancyClient>();
            _employerVacancyClient = new Mock<IEmployerVacancyClient>();
            _utility = new Mock<IUtility>();
            _orchestrator = new ApplicationReviewOrchestrator(_employerVacancyClient.Object, _recruitVacancyClient.Object, _utility.Object);
        }

        [Test]
        public async Task PostApplicationReviewStatusChangeModelAsync_ReturnsApplicantsFullName()
        {
            // Arrange
            var model = _fixture.Create<ApplicationReviewStatusChangeModel>();
            var routeModel = _fixture.Create<VacancyRouteModel>();
            var vacancyUser = _fixture.Create<VacancyUser>();

            _utility.Setup(x => x.GetAuthorisedApplicationReviewAsync(model))
                .ReturnsAsync(new ApplicationReview
                {
                    Id = model.ApplicationReviewId,
                    CandidateFeedback = model.CandidateFeedback,
                    Application = new Application
                    {
                        FirstName = "Jack",
                        LastName = "Sparrow"
                    }
                });
            _employerVacancyClient.Setup(x => x.SetApplicationReviewStatus(model.ApplicationReviewId, model.Outcome, model.CandidateFeedback, vacancyUser))
                .Returns(Task.CompletedTask);

            // Act
            var applicantFullName = await _orchestrator.PostApplicationReviewStatusChangeModelAsync(model, vacancyUser);

            // Assert
            Assert.IsNotNull(applicantFullName);
            Assert.AreEqual("Jack Sparrow", applicantFullName);
        }

        [Test]
        public async Task GetApplicationReviewFeedBackViewModelAsync_ReturnsCandidateName()
        {
            var model = _fixture.Create<ApplicationReviewFeedBackViewModel>();
            var routeModel = _fixture.Create<VacancyRouteModel>();
            var vacancyUser = _fixture.Create<VacancyUser>();

            var applicationReview = _fixture.Create<ApplicationReview>();

            _utility.Setup(x => x.GetAuthorisedApplicationReviewAsync(model))
                .ReturnsAsync(applicationReview);

            _employerVacancyClient.Setup(x => x.SetApplicationReviewStatus(model.ApplicationReviewId, model.Outcome, model.CandidateFeedback, vacancyUser))
                .Returns(Task.CompletedTask);

            string result = await _orchestrator.GetApplicationReviewFeedBackViewModelAsync(model);

            Assert.IsNotNull(result);
            Assert.AreEqual(applicationReview.Application.FullName, result);
        }

        [Test]
        public async Task GetApplicationReviewFeedBackViewModelAsync_ReturnsCandidateInfo()
        {
            var model = _fixture.Create<ApplicationReviewEditModel>();
            var routeModel = _fixture.Create<VacancyRouteModel>();
            var vacancyUser = _fixture.Create<VacancyUser>();

            var applicationReview = _fixture.Create<ApplicationReview>();
            applicationReview.IsWithdrawn = false;

            _utility.Setup(x => x.GetAuthorisedApplicationReviewAsync(model))
                .ReturnsAsync(applicationReview);

            _employerVacancyClient.Setup(x => x.SetApplicationReviewStatus(model.ApplicationReviewId, model.Outcome, model.CandidateFeedback, vacancyUser))
                .Returns(Task.CompletedTask);

            var result = await _orchestrator.GetApplicationReviewFeedBackViewModelAsync(model);

            Assert.IsNotNull(result);
            Assert.AreEqual(applicationReview.Application.FullName, result.Name);
            Assert.AreEqual(model.ApplicationReviewId, result.ApplicationReviewId);
            Assert.AreEqual(model.Ukprn, result.Ukprn);
            Assert.AreEqual(model.VacancyId, result.VacancyId);
            Assert.AreEqual(model.CandidateFeedback, result.CandidateFeedback);
        }
    }
}
