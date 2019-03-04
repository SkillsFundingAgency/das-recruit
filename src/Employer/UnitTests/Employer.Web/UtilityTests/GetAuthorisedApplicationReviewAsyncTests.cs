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
            var vacancyId = Guid.NewGuid(); 

            var client = new Mock<IRecruitVacancyClient>();
            client.Setup(c => c.GetApplicationReviewAsync(applicationReviewId)).Returns(Task.FromResult(
                new ApplicationReview
                {
                    Id = applicationReviewId,                    
                    VacancyReference = 1000000001
                }));

            client.Setup(c=>c.GetVacancyAsync(vacancyId)).Returns(Task.FromResult(
                new Vacancy() {
                    EmployerAccountId = applicationReviewEmployerAccountId,
                    VacancyReference = 1000000001,
                    Id = vacancyId
                }));

            var rm = new ApplicationReviewRouteModel
            {
                EmployerAccountId = requestedEmployerAccountId,
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
                ex.Result.Message.Should().Be("The employer account 'ANOTHER EMPLOYER ACCOUNT ID' " +
                                              "cannot access employer account 'EMPLOYER ACCOUNT ID' " +
                                              $"application '{rm.ApplicationReviewId}' for " +
                                              $"vacancy '{vacancyId}'.");              
            }
        }
    }
}
