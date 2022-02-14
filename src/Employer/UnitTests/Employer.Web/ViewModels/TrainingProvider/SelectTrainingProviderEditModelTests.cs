using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.TrainingProvider;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.ViewModels.TrainingProvider
{
    public class SelectTrainingProviderEditModelTests
    {
        

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ShouldErrorIfUkprnIsNotSpecified(string inputUkprn)
        {
            var vm = new SelectTrainingProviderEditModel {
                Ukprn = inputUkprn,
                SelectionType = TrainingProviderSelectionType.Ukprn
            };

            var validator = new SelectTrainingProviderEditModelValidator();

            var result = validator.Validate(vm);

            result.IsValid.Should().BeFalse();
            result.Errors.Count.Should().Be(1);
            result.Errors[0].PropertyName.Should().Be(nameof(SelectTrainingProviderEditModel.Ukprn));
            result.Errors[0].ErrorMessage.Should().Be("You must provide a UKPRN");
        }

        [Fact]
        public void ShouldErrorIfUkprnIsInvalid()
        {
            var vm = new SelectTrainingProviderEditModel {
                Ukprn = "invalid ukprn",
                SelectionType = TrainingProviderSelectionType.Ukprn
            };

            var validator = new SelectTrainingProviderEditModelValidator();

            var result = validator.Validate(vm);

            result.IsValid.Should().BeFalse();
            result.Errors.Count.Should().Be(1);
            result.Errors[0].PropertyName.Should().Be(nameof(SelectTrainingProviderEditModel.Ukprn));
            result.Errors[0].ErrorMessage.Should().Be("UKPRN is not recognised");
        }

        [Fact]
        public void ShouldBeValidIfUkprnSpecified()
        {
            var vm = new SelectTrainingProviderEditModel {
                Ukprn = "12345678",
                SelectionType = TrainingProviderSelectionType.Ukprn
            };

            var validator = new SelectTrainingProviderEditModelValidator();

            var result = validator.Validate(vm);

            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ShouldErrorIfTrainingProviderSearchIsNotSpecified(string inputUkprn)
        {
            var vm = new SelectTrainingProviderEditModel {
                TrainingProviderSearch = inputUkprn,
                SelectionType = TrainingProviderSelectionType.TrainingProviderSearch
            };

            var validator = new SelectTrainingProviderEditModelValidator();

            var result = validator.Validate(vm);

            result.IsValid.Should().BeFalse();
            result.Errors.Count.Should().Be(1);
            result.Errors[0].PropertyName.Should().Be(nameof(SelectTrainingProviderEditModel.TrainingProviderSearch));
            result.Errors[0].ErrorMessage.Should().Be("Please select a training provider");
        }
        
        [Fact]
        public void ShouldBeValidIfTrainingProviderSearchSpecified()
        {
            var vm = new SelectTrainingProviderEditModel {
                TrainingProviderSearch = "something specified",
                SelectionType = TrainingProviderSelectionType.TrainingProviderSearch
            };

            var validator = new SelectTrainingProviderEditModelValidator();

            var result = validator.Validate(vm);

            result.IsValid.Should().BeTrue();
        }
    }
}
