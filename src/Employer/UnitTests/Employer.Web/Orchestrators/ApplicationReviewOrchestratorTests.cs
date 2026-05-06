using System.Collections.Generic;
using Esfa.Recruit.Employer.Web;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.ApplicationReview;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Orchestrators
{
    public class ApplicationReviewOrchestratorTests
    {
        private Fixture _fixture;
        private Mock<IEmployerVacancyClient> _employerVacancyClient;
        private Mock<IRecruitVacancyClient> _vacancyClient;
        private Mock<IUtility> _utility;
        private ApplicationReviewOrchestrator _orchestrator;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _employerVacancyClient = new Mock<IEmployerVacancyClient>();
            _vacancyClient = new Mock<IRecruitVacancyClient>();
            _utility = new Mock<IUtility>();
            _orchestrator = new ApplicationReviewOrchestrator(_employerVacancyClient.Object, _vacancyClient.Object, _utility.Object);
        }

        [Test]
        public async Task PostApplicationReviewEditModelAsync_ReturnsCandidateInfo()
        {
            var model = _fixture.Create<ApplicationReviewEditModel>();
            var vacancyUser = _fixture.Create<VacancyUser>();

            var applicationReview = _fixture.Create<ApplicationReview>();

            _utility.Setup(x => x.GetAuthorisedApplicationReviewAsync(model))
                .ReturnsAsync(applicationReview);
            _employerVacancyClient.Setup(x => x.SetApplicationReviewStatus(model.ApplicationReviewId, model.Outcome, model.CandidateFeedback, vacancyUser))
                .ReturnsAsync(false);

            var result = await _orchestrator.PostApplicationReviewEditModelAsync(model, vacancyUser);

            Assert.That(applicationReview.Id, Is.EqualTo(result.ApplicationReviewId));
            Assert.That(applicationReview.GetFriendlyId(), Is.EqualTo(result.FriendlyId));
            Assert.That(applicationReview.Application.FullName, Is.EqualTo(result.Name));
        }

        [Test]
        [MoqInlineAutoData(ApplicationReviewStatus.Successful, true)]
        [MoqInlineAutoData(ApplicationReviewStatus.Unsuccessful, true)]
        [MoqInlineAutoData(ApplicationReviewStatus.AllShared, false)]
        [MoqInlineAutoData(ApplicationReviewStatus.InReview, false)]
        [MoqInlineAutoData(ApplicationReviewStatus.Interviewing, false)]
        [MoqInlineAutoData(ApplicationReviewStatus.EmployerInterviewing, false)]
        [MoqInlineAutoData(ApplicationReviewStatus.EmployerUnsuccessful, false)]
        [MoqInlineAutoData(ApplicationReviewStatus.PendingShared, false)]
        [MoqInlineAutoData(ApplicationReviewStatus.PendingToMakeUnsuccessful, false)]
        [MoqInlineAutoData(ApplicationReviewStatus.New, false)]
        [MoqInlineAutoData(ApplicationReviewStatus.Shared, false)]
        public async Task IsAllApplicationReviewsHasOutcomeAsync_Returns_Valid(ApplicationReviewStatus status, bool expectedResult)
        {
            var routeModel = _fixture.Create<VacancyRouteModel>();
            var model = _fixture.Build<ApplicationReviewEditModel>()
                .With(x => x.VacancyId, routeModel.VacancyId)
                .Create();
            var applicationReviews = _fixture.Create<List<ApplicationReview>>();
            applicationReviews.ForEach(ar =>
            {
                ar.IsWithdrawn = false;
                ar.Status = status;
            });

            _vacancyClient.Setup(x => x.GetApplicationReviewsAsync(routeModel.VacancyId))
                .ReturnsAsync(applicationReviews);

            var result = await _orchestrator.IsAllApplicationReviewsHasOutcomeAsync(model.VacancyId);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.EqualTo(expectedResult));
        }
    }
}
