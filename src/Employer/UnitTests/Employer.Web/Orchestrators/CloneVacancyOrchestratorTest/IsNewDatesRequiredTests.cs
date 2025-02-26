using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.UnitTests.Employer.Web.Orchestrators.CloneVacancyOrchestratorTest
{
    public class IsNewDatesRequiredTests : CloneVacancyOrchestratorTestBase
    {
        [Test]
        public void WhenStatusIsLiveAndDatesAreInFuture_ThenReturnFalse()
        {
            var sut = GetSut(SourceVacancy);
            var vacancy = new Vacancy{Status = VacancyStatus.Live, ClosingDate = DateTime.UtcNow.AddDays(1)};
            sut.IsNewDatesRequired(vacancy).Should().BeFalse();
        }

        [Test]
        public void WhenStatusIsLiveAndDatesAreInPast_ThenReturnTrue()
        {
            var sut = GetSut(SourceVacancy);
            var vacancy = new Vacancy{Status = VacancyStatus.Live, ClosingDate = DateTime.UtcNow.AddDays(-1)};
            sut.IsNewDatesRequired(vacancy).Should().BeTrue();
        }

    }
}