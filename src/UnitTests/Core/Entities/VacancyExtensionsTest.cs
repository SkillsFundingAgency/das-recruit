using Esfa.Recruit.Storage.Client.Domain.Entities;
using Esfa.Recruit.Storage.Client.Domain.Enum;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.Employer.UnitTests.Core.Entities
{
    public class VacancyExtensionsTest
    {
        [Fact]
        public void ShouldBeSubmitableIfStatusIsDraft()
        {
            var vacancy = new Vacancy
            {
                Status = VacancyStatus.Draft
            };

            vacancy.IsSubmittable().Should().BeTrue();
        }

        [Fact]
        public void ShouldNotBeSubmitableIfStatusIsNotDraft()
        {
            var vacancy = new Vacancy
            {
                Status = VacancyStatus.Submitted
            };

            vacancy.IsSubmittable().Should().BeFalse();
        }
    }
}
