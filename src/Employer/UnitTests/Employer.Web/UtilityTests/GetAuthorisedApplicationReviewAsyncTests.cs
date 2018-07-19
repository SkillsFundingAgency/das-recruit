using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using FluentAssertions;
using Moq;
using Xunit;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.UtilityTests
{
    public class GetAuthorisedApplicationReviewAsyncTests
    {
        [Theory]
        [InlineData("EMPLOYER ACCOUNT ID", "EMPLOYER ACCOUNT ID", true)]
        [InlineData("EMPLOYER ACCOUNT ID", "ANOTHER EMPLOYER ACCOUNT ID", false)]
        public void GetAuthorisedApplicationReviewAsync_ShouldAllowForEmployerAccountId(string applicationReviewEmployerAccountId,
            string requestedEmployerAccountId, bool shouldAllow)
        {
            var applicationReviewId = Guid.NewGuid();

            var client = new Mock<IEmployerVacancyClient>();
            client.Setup(c => c.GetApplicationReviewAsync(applicationReviewId)).Returns(Task.FromResult(
                new ApplicationReview
                {
                    Id = applicationReviewId,
                    EmployerAccountId = applicationReviewEmployerAccountId,
                    VacancyReference = 1000000001
                }));

            var rm = new ApplicationReviewRouteModel
            {
                EmployerAccountId = requestedEmployerAccountId,
                ApplicationReviewId = applicationReviewId
            };

            Func<Task<ApplicationReview>> act = () => Utility.GetAuthorisedApplicationReviewAsync(client.Object, rm);

            if (shouldAllow)
            {
                var applicationReview = act().Result;
                applicationReview.EmployerAccountId.Should().Be(requestedEmployerAccountId);
            }
            else
            {
                var ex = Assert.ThrowsAsync<AuthorisationException>(act);
                ex.Result.Message.Should().Be($"The employer account 'ANOTHER EMPLOYER ACCOUNT ID' cannot access employer account 'EMPLOYER ACCOUNT ID' application '{applicationReviewId}' for vacancy '1000000001'.");
            }
        }
    }
}
