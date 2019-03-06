﻿using System;
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
        private readonly Guid _applicationReviewId;
        private readonly Guid _vacancyId;
        private const string ApplicationReviewEmployerAccountId = "EMPLOYER ACCOUNT ID";
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
            new Vacancy
            {
                EmployerAccountId = ApplicationReviewEmployerAccountId,
                Id = _vacancyId
            });
        }

        [Fact]
        public async Task GetAuthorisedApplicationReviewAsync_ShouldAllowForAssociatedEmployerAccountId()
        {
            const string requestedEmployerAccountId = "EMPLOYER ACCOUNT ID";

            var rm = new ApplicationReviewRouteModel
            {
                EmployerAccountId = requestedEmployerAccountId,
                ApplicationReviewId = _applicationReviewId,
                VacancyId = _vacancyId
            };

            var applicationReview = await Utility.GetAuthorisedApplicationReviewAsync(_mockVacancyClient.Object, rm);
            applicationReview.Should().NotBeNull();
        }

        [Fact]
        public async Task GetAuthorisedApplicationReviewAsync_ShouldNotAllowForUnassociatedEmployerAccountId()
        {
            const string requestedEmployerAccountId = "WRONG EMPLOYER ACCOUNT ID";

            var rm = new ApplicationReviewRouteModel
            {
                EmployerAccountId = requestedEmployerAccountId,
                ApplicationReviewId = _applicationReviewId,
                VacancyId = _vacancyId
            };

            Func<Task<ApplicationReview>> act = () => Utility.GetAuthorisedApplicationReviewAsync(_mockVacancyClient.Object, rm);

                var ex = await Assert.ThrowsAsync<AuthorisationException>(act);
                ex.Message.Should().Be($"The employer account '{requestedEmployerAccountId}' " +
                                              $"cannot access employer account '{ApplicationReviewEmployerAccountId}' " +
                                              $"application '{rm.ApplicationReviewId}' for " +
                                              $"vacancy '{_vacancyId}'.");
        }
    }
}
