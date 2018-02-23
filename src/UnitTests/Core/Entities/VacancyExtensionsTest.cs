using Esfa.Recruit.Storage.Client.Domain;
using Esfa.Recruit.Storage.Client.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.Employer.UnitTests.Core.Entities
{
    public class VacancyExtensionsTest
    {
        [Fact]
        public void ShouldBeAbleToSubmitIfStatusIsDraft()
        {
            var vacancy = new Vacancy
            {
                Status = VacancyStatus.Draft
            };

            vacancy.CanSubmit.Should().BeTrue();
        }

        [Fact]
        public void ShouldNotBeAbleToSubmitIfStatusIsNotDraft()
        {
            var vacancy = new Vacancy
            {
                Status = VacancyStatus.Submitted
            };

            vacancy.CanSubmit.Should().BeFalse();
        }
    }
}
