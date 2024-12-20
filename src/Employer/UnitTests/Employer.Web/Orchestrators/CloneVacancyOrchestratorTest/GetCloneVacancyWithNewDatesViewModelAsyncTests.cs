using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.UnitTests.Employer.Web.Orchestrators.CloneVacancyOrchestratorTest;

namespace UnitTests.Employer.Web.Orchestrators.CloneVacancyOrchestratorTest
{
    public class GetCloneVacancyWithNewDatesViewModelAsyncTests : CloneVacancyOrchestratorTestBase
    {
        [Test]
        public async Task WhenDatesAreInPast_ThenModelShouldHaveEmptyDates()
        {
            var vacancy = SourceVacancy;
            vacancy.ClosingDate = DateTime.UtcNow.AddDays(-1);

            var sut = GetSut(vacancy);
            var vm = await sut.GetCloneVacancyWithNewDatesViewModelAsync(VRM);

            vm.IsNewDatesForced.Should().BeTrue();
            vm.Title.Should().Be(CloneVacancyOrchestrator.ChangeBothDatesTitle);
            vm.StartDate.Should().BeNull();
            vm.ClosingDate.Should().BeNull();
        }

        [Test]
        public async Task WhenDatesAreInFuture_ThenModelShouldBePopulatedWithDate()
        {
            var sut = GetSut(SourceVacancy);
            var vm = await sut.GetCloneVacancyWithNewDatesViewModelAsync(VRM);

            vm.IsNewDatesForced.Should().BeFalse();
            vm.Title.Should().Be(CloneVacancyOrchestrator.ChangeEitherDatesTitle);
            vm.StartDay.Should().Be($"{SourceStartDate.Day:00}");
            vm.StartMonth.Should().Be($"{SourceStartDate.Month:00}");
            vm.StartYear.Should().Be($"{SourceStartDate.Year:00}");
            vm.ClosingDay.Should().Be($"{SourceClosingDate.Day:00}");
            vm.ClosingMonth.Should().Be($"{SourceClosingDate.Month:00}");
            vm.ClosingYear.Should().Be($"{SourceClosingDate.Year:00}");
        }
    }
}