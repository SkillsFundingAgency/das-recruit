using Esfa.Recruit.Employer.Web;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.UtilityTests
{
    public class GetAuthorisedApplicationReviewAsyncTests
    {
        private readonly Guid _applicationReviewId;
        private readonly Guid _vacancyId;
        private const string ApplicationReviewEmployerAccountId = "EMPLOYER ACCOUNT ID";
        private readonly Mock<IRecruitVacancyClient> _mockVacancyClient;
        private readonly Utility _utility;

        public GetAuthorisedApplicationReviewAsyncTests()
        {   
            _applicationReviewId = Guid.NewGuid();
            _vacancyId = Guid.NewGuid(); 

            _mockVacancyClient = new Mock<IRecruitVacancyClient>();
            _mockVacancyClient
            .Setup(c => c.GetApplicationReviewAsync(_applicationReviewId))
            .ReturnsAsync(new ApplicationReview { Id = _applicationReviewId });

            _mockVacancyClient
            .Setup(c => c.GetVacancyAsync(_vacancyId))
            .ReturnsAsync(
            new Vacancy
            {
                EmployerAccountId = ApplicationReviewEmployerAccountId,
                Id = _vacancyId
            });

            _utility = new Utility(_mockVacancyClient.Object);
        }

        [Test]
        public async Task GetAuthorisedApplicationReviewAsync_ShouldAllowForAssociatedEmployerAccountId()
        {
            const string requestedEmployerAccountId = "EMPLOYER ACCOUNT ID";

            var rm = new ApplicationReviewRouteModel
            {
                EmployerAccountId = requestedEmployerAccountId,
                ApplicationReviewId = _applicationReviewId,
                VacancyId = _vacancyId
            };

            var applicationReview = await _utility.GetAuthorisedApplicationReviewAsync(rm);
            applicationReview.Should().NotBeNull();
        }

        [Test]
        public async Task GetAuthorisedApplicationReviewAsync_ShouldNotAllowForUnassociatedEmployerAccountId()
        {
            const string requestedEmployerAccountId = "WRONG EMPLOYER ACCOUNT ID";

            var rm = new ApplicationReviewRouteModel
            {
                EmployerAccountId = requestedEmployerAccountId,
                ApplicationReviewId = _applicationReviewId,
                VacancyId = _vacancyId
            };

            Func<Task<ApplicationReview>> act = () => _utility.GetAuthorisedApplicationReviewAsync(rm);

            var ex = await act.Should()
                .ThrowAsync<AuthorisationException>()
                .WithMessage(
                    $"The employer account '{requestedEmployerAccountId}' " +
                    $"cannot access employer account '{ApplicationReviewEmployerAccountId}' " +
                    $"application '{rm.ApplicationReviewId}' for " +
                    $"vacancy '{_vacancyId}'.");
        }
    }
}
