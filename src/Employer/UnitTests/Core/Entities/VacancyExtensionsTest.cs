using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Employer.UnitTests.Core.Entities
{
    public class VacancyExtensionsTest
    {
        [Test]
        public void ShouldBeAbleToSubmitIfStatusIsDraft()
        {
            var vacancy = new Vacancy
            {
                Status = VacancyStatus.Draft
            };

            vacancy.CanSubmit.Should().BeTrue();
        }

        [Test]
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
