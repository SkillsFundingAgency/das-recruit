using Esfa.Recruit.Employer.Web.ViewModels.Part1.TrainingProvider;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.ViewModels.TrainingProvider
{
    public class ConfirmTrainingProviderEditModelTests
    {
        private readonly Guid _dummyVacancyGuid = Guid.NewGuid();

        [TestCase(null)]
        [TestCase("")]
        public void ShouldErrorIfUkprnIsNotSpecified(string inputUkprn)
        {
            var vm = new ConfirmTrainingProviderEditModel
            {
                Ukprn = inputUkprn
            };

            var validator = new ConfirmTrainingProviderEditModelValidator();

            var result = validator.Validate(vm);

            result.IsValid.Should().BeFalse();
            result.Errors.Count.Should().Be(1);
            result.Errors[0].PropertyName.Should().Be("Ukprn");
            result.Errors[0].ErrorMessage.Should().Be("The UKPRN field is required");
        }

        [Test]
        public void ShouldErrorIfUkprnIsInvalid()
        {
            var vm = new ConfirmTrainingProviderEditModel {
                Ukprn = "invalid ukprn"
            };

            var validator = new ConfirmTrainingProviderEditModelValidator();

            var result = validator.Validate(vm);

            result.IsValid.Should().BeFalse();
            result.Errors.Count.Should().Be(1);
            result.Errors[0].PropertyName.Should().Be("Ukprn");
            result.Errors[0].ErrorMessage.Should().Be("UKPRN is not recognised");
        }

        [Test]
        public void ShouldBeValidIfUkprnSpecified()
        {
            var vm = new ConfirmTrainingProviderEditModel
            {
                Ukprn = "12345678"
            };

            var validator = new ConfirmTrainingProviderEditModelValidator();

            var result = validator.Validate(vm);

            result.IsValid.Should().BeTrue();
        }
    }
}
