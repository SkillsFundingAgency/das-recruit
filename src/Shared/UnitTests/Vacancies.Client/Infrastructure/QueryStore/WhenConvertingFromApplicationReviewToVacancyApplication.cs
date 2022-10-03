using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;
using FluentAssertions;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.QueryStore
{
    public class WhenConvertingFromApplicationReviewToVacancyApplication
    {
        [Test, AutoData]
        public void Then_The_Fields_Are_Mapped(ApplicationReview source)
        {
            //Arrange
            source.IsWithdrawn = false;
            
            //Act
            var actual = (VacancyApplication) source;

            //Assert
            actual.CandidateId.Should().Be(source.CandidateId);
            actual.Status.Should().Be(source.Status);
            actual.SubmittedDate.Should().Be(source.SubmittedDate);
            actual.ApplicationReviewId.Should().Be(source.Id);
            actual.IsWithdrawn.Should().Be(source.IsWithdrawn);
            actual.FirstName.Should().Be(source.Application.FirstName);
            actual.LastName.Should().Be(source.Application.LastName);
            actual.DateOfBirth.Should().Be(source.Application.BirthDate);
            actual.DisabilityStatus.Should().Be(actual.DisabilityStatus);
        }

        [Test, AutoData]
        public void Then_If_Withdrawn_Then_Name_And_Dob_Not_Populated(ApplicationReview source)
        {
            //Arrange
            source.IsWithdrawn = true;
            
            //Act
            var actual = (VacancyApplication) source;
            
            //Assert
            actual.CandidateId.Should().Be(source.CandidateId);
            actual.Status.Should().Be(source.Status);
            actual.SubmittedDate.Should().Be(source.SubmittedDate);
            actual.ApplicationReviewId.Should().Be(source.Id);
            actual.IsWithdrawn.Should().Be(source.IsWithdrawn);
            actual.FirstName.Should().BeNullOrEmpty();
            actual.LastName.Should().BeNullOrEmpty();
            actual.DateOfBirth.Should().BeNull();
            actual.DisabilityStatus.Should().Be(ApplicationReviewDisabilityStatus.Unknown);
        }
    }
}