using Esfa.Recruit.Shared.Web.ViewModels;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.ViewModels.Part1
{
    public class PartOnePageInfoViewModelTests
    {
        [Theory]
        [InlineData("True", false, false, true)]
        [InlineData("False", false, false, true)]
        [InlineData("non-bool value", false, false, true)]
        [InlineData(null, false, false, true)]
        [InlineData("True", true, false, true)]
        [InlineData("False", true, false, false)]
        [InlineData("non-bool value", true, false, true)]
        [InlineData(null, true, false, true)]
        [InlineData("True", true, true, false)]
        [InlineData("False", true, true, false)]
        [InlineData("non-bool value", true, true, false)]
        [InlineData(null, true, true, false)]
        public void SetWizard(string requestedWizardParam, bool hasCompletedPartOne, bool hasStartedPartTwo, bool expectedIsWizardValue)
        {
            var vm = new PartOnePageInfoViewModel
            {
                HasCompletedPartOne = hasCompletedPartOne,
                HasStartedPartTwo = hasStartedPartTwo
            };

            vm.SetWizard(requestedWizardParam);

            vm.IsWizard.Should().Be(expectedIsWizardValue);
            vm.IsNotWizard.Should().Be(!expectedIsWizardValue);
        }
    }
}
