using System;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web;
using Esfa.Recruit.Provider.Web.RouteModel;
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
        [Theory]
        [InlineData(12345678, 12345678, true)]
        [InlineData(12345678, 123456789, false)]
        public async Task GetAuthorisedApplicationReviewAsync_ShouldAllowForProviderUKPRN(long applicationReviewUkprn,
            long requestedUkprn, bool shouldAllow)
        {
            var applicationReviewId = Guid.NewGuid();
            var vacancyId = Guid.NewGuid(); 
            var provider=new TrainingProvider()
            {
                Ukprn = applicationReviewUkprn
            };
            var client = new Mock<IRecruitVacancyClient>();
            client.Setup(c => c.GetApplicationReviewAsync(applicationReviewId)).Returns(Task.FromResult(
                new ApplicationReview
                {
                    Id = applicationReviewId,                    
                    VacancyReference = 1000000001
                }));

            client.Setup(c=>c.GetVacancyAsync(vacancyId)).Returns(Task.FromResult(
                new Vacancy() {
                    VacancyReference = 1000000001,
                    Id = vacancyId,
                    TrainingProvider = provider
                }));

            var rm = new ApplicationReviewRouteModel
            {
                Ukprn = requestedUkprn,
                ApplicationReviewId = applicationReviewId,
                VacancyId = vacancyId
            };

            Func<Task<ApplicationReview>> act = () => Utility.GetAuthorisedApplicationReviewAsync(client.Object, rm);

            if (shouldAllow)
            {
                var applicationReview = act().Result;
                applicationReview.Should().NotBeNull();                
            }
            else
            {
                var ex = Assert.ThrowsAsync<AuthorisationException>(act);
                ex.Result.Message.Should().Be(
                    $"The provider account '{requestedUkprn}' cannot access provider account '{applicationReviewUkprn}' " +
                    $"application '{rm.ApplicationReviewId}' for vacancy '{vacancyId}'.");
            }
        }
    }
}
