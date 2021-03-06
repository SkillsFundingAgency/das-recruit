using System;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.UnitTests.Provider.Web.Orchestrators.CloneVacancyOrchestratorTest;
using FluentAssertions;
using Xunit;

namespace UnitTests.Provider.Web.Orchestrators.CloneVacancyOrchestratorTest
{
    public class GetCloneVacancyWithNewDatesViewModelAsyncTests : CloneVacancyOrchestratorTestBase
    {
        [Fact]
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

        [Fact]
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