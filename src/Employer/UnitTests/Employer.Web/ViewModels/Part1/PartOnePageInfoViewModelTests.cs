using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.ViewModels.Part1
{
    public class PartOnePageInfoViewModelTests
    {
        [TestCase("True", false, false, true)]
        [TestCase("False", false, false, true)]
        [TestCase("non-bool value", false, false, true)]
        [TestCase(null, false, false, true)]
        [TestCase("True", true, false, true)]
        [TestCase("False", true, false, false)]
        [TestCase("non-bool value", true, false, true)]
        [TestCase(null, true, false, true)]
        [TestCase("True", true, true, false)]
        [TestCase("False", true, true, false)]
        [TestCase("non-bool value", true, true, false)]
        [TestCase(null, true, true, false)]
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
