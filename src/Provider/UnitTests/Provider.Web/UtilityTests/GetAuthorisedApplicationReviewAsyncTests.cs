using System;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Shared.Web.FeatureToggle;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using FluentAssertions;
using Moq;
using Xunit;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.UtilityTests
{
    public class GetAuthorisedApplicationReviewAsyncTests
    {
        private readonly Guid _applicationReviewId;
        private readonly Guid _vacancyId;
        private long _applicationReviewUkprn = 12345678;
        private readonly Mock<IRecruitVacancyClient> _mockVacancyClient;

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
            new Vacancy()
            {
                Id = _vacancyId,
                TrainingProvider = new TrainingProvider { Ukprn = _applicationReviewUkprn },
                OwnerType = OwnerType.Provider
            });
        }

        [Fact]
        public async Task GetAuthorisedApplicationReviewAsync_ShouldAllowForProviderAssociatedUkprn()
        {
            const long requestedUkprn = 12345678;
            var utility = new Utility(_mockVacancyClient.Object, Mock.Of<IFeature>());
            var rm = new ApplicationReviewRouteModel
            {
                Ukprn = requestedUkprn,
                ApplicationReviewId = _applicationReviewId,
                VacancyId = _vacancyId
            };

            var applicationReview = await utility.GetAuthorisedApplicationReviewAsync(rm);
            applicationReview.Should().NotBeNull();
        }

        [Fact]
        public async Task GetAuthorisedApplicationReviewAsync_ShouldNotAllowForProviderUnassociatedUkprn()
        {
            const long requestedUkprn = 123456789;
            var utility = new Utility(_mockVacancyClient.Object, Mock.Of<IFeature>());
            var rm = new ApplicationReviewRouteModel
            {
                Ukprn = requestedUkprn,
                ApplicationReviewId = _applicationReviewId,
                VacancyId = _vacancyId
            };

            var ex = await Assert.ThrowsAsync<AuthorisationException>(() => utility.GetAuthorisedApplicationReviewAsync(rm));
            ex.Message.Should().Be(
                $"The provider account '{requestedUkprn}' cannot access provider account '{_applicationReviewUkprn}' " +
                $"application '{rm.ApplicationReviewId}' for vacancy '{_vacancyId}'.");
        }
    }
}