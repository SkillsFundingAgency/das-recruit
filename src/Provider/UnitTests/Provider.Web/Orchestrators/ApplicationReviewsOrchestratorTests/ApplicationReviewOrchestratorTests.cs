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
                .ReturnsAsync(false);

            // Act
            var applicationReviewStatusChangeInfo = await _orchestrator.PostApplicationReviewStatusChangeModelAsync(model, vacancyUser);

            // Assert
            Assert.IsNotNull(applicationReviewStatusChangeInfo);
            Assert.AreEqual("Jack Sparrow", applicationReviewStatusChangeInfo.CandidateName);
            Assert.AreEqual(false, applicationReviewStatusChangeInfo.ShouldMakeOthersUnsuccessful);
        }

        [Test]
        public async Task GetApplicationReviewFeedbackViewModelAsync_ReturnsCandidateName()
        {
            var model = _fixture.Create<ApplicationReviewFeedbackViewModel>();
            var routeModel = _fixture.Create<VacancyRouteModel>();
            var vacancyUser = _fixture.Create<VacancyUser>();

            var applicationReview = _fixture.Create<ApplicationReview>();

            _utility.Setup(x => x.GetAuthorisedApplicationReviewAsync(model))
                .ReturnsAsync(applicationReview);

            string result = await _orchestrator.GetApplicationReviewFeedbackViewModelAsync(model);

            Assert.IsNotNull(result);
            Assert.AreEqual(applicationReview.Application.FullName, result);
        }

        [Test]
        public async Task GetApplicationReviewFeedbackViewModelAsync_ReturnsCandidateInfo()
        {
            var routeModel = _fixture.Create<VacancyRouteModel>();
            var model = _fixture.Build<ApplicationReviewEditModel>()
                .With(x => x.VacancyId, routeModel.VacancyId)
                .Create();
            var vacancyUser = _fixture.Create<VacancyUser>();
            var vacancy = _fixture.Build<Vacancy>()
                .With(x => x.Id, routeModel.VacancyId)
                .Create();

            var applicationReview = _fixture.Create<ApplicationReview>();
            applicationReview.IsWithdrawn = false;

            _utility.Setup(x => x.GetAuthorisedApplicationReviewAsync(model))
                .ReturnsAsync(applicationReview);
            _recruitVacancyClient.Setup(x => x.GetVacancyAsync(routeModel.VacancyId.Value))
                .ReturnsAsync(vacancy);

            var result = await _orchestrator.GetApplicationReviewFeedbackViewModelAsync(model);

            Assert.IsNotNull(result);
            Assert.AreEqual(applicationReview.Application.FullName, result.Name);
            Assert.AreEqual(model.ApplicationReviewId, result.ApplicationReviewId);
            Assert.AreEqual(model.Ukprn, result.Ukprn);
            Assert.AreEqual(model.VacancyId, result.VacancyId);
            Assert.AreEqual(model.CandidateFeedback, result.CandidateFeedback);
        }
    }
}
