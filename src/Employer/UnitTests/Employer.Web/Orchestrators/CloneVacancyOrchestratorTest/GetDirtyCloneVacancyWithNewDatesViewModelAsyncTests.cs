using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.ViewModels.CloneVacancy;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.UnitTests.Employer.Web.Orchestrators.CloneVacancyOrchestratorTest
{
    public class GetDirtyCloneVacancyWithNewDatesViewModelAsyncTests : CloneVacancyOrchestratorTestBase
    {
        [Fact]
        public async Task ThenReturnUpdatedModel()
        {
            var dirtyModel = new CloneVacancyWithNewDatesEditModel()
            {
                EmployerAccountId = EmployerAccountId,
                ClosingDay = "1",
                ClosingMonth = "11",
                ClosingYear = "20122",
                StartDay = "1",
                StartMonth = "2",
                StartYear = "yyyy"
            };
            
            var sut = GetSut(SourceVacancy);

            var vm = await sut.GetDirtyCloneVacancyWithNewDatesViewModelAsync(dirtyModel);

            vm.StartDay.Should().Be(dirtyModel.StartDay);
            vm.StartMonth.Should().Be(dirtyModel.StartMonth);
            vm.StartYear.Should().Be(dirtyModel.StartYear);
            vm.StartDay.Should().Be(dirtyModel.StartDay);
            vm.ClosingDay.Should().Be(dirtyModel.ClosingDay);
            vm.ClosingMonth.Should().Be(dirtyModel.ClosingMonth);
            vm.ClosingMonth.Should().Be(dirtyModel.ClosingMonth);
        }
    }
}