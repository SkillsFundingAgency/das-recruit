using Esfa.Recruit.Provider.Web;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Shared.Web.Domain;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using NUnit.Framework;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.UtilityTests;

public class GetAuthorisedApplicationReviewAsyncTests
{
    private Guid _applicationReviewId;
    private Guid _vacancyId;
    private long _applicationReviewUkprn = 12345678;
    private Mock<IRecruitVacancyClient> _mockVacancyClient;

    [SetUp]
    public void Setup()
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
                    OwnerType = OwnerType.Provider,
                    DisabilityConfident = DisabilityConfident.No, 
                });
    }

    [Test]
    public async Task GetAuthorisedApplicationReviewAsync_ShouldAllowForProviderAssociatedUkprn()
    {
        const long requestedUkprn = 12345678;
        var utility = new Utility(_mockVacancyClient.Object, Mock.Of<ITaskListValidator>());
        var rm = new ApplicationReviewRouteModel
        {
            Ukprn = requestedUkprn,
            ApplicationReviewId = _applicationReviewId,
            VacancyId = _vacancyId,
        };

        var applicationReview = await utility.GetAuthorisedApplicationReviewAsync(rm);
        applicationReview.Should().NotBeNull();
    }

    [Test]
    public async Task GetAuthorisedApplicationReviewAsync_ShouldNotAllowForProviderUnassociatedUkprn()
    {
        const long requestedUkprn = 123456789;
        var utility = new Utility(_mockVacancyClient.Object, Mock.Of<ITaskListValidator>());
        var rm = new ApplicationReviewRouteModel
        {
            Ukprn = requestedUkprn,
            ApplicationReviewId = _applicationReviewId,
            VacancyId = _vacancyId
        };
        
        // act/assert
        var func = async () => await utility.GetAuthorisedApplicationReviewAsync(rm);
        var ex = await func.Should().ThrowAsync<AuthorisationException>();
        ex.And.Message.Should().Be(
            $"The provider account '{requestedUkprn}' cannot access provider account '{_applicationReviewUkprn}' " +
            $"application '{rm.ApplicationReviewId}' for vacancy '{_vacancyId}'.");
    }
}