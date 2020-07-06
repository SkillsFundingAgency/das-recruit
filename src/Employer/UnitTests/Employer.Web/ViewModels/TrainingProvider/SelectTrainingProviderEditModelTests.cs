using Esfa.Recruit.Employer.Web.ViewModels.Part1.TrainingProvider;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.ViewModels.TrainingProvider
{
    public class SelectTrainingProviderEditModelTests
    {
        [Fact]
        public void ShouldErrorIfSelectTrainingProviderNotSpecified()
        {
            var vm = new SelectTrainingProviderEditModel
            {
                IsTrainingProviderSelected = null,
            };

            var validator = new SelectTrainingProviderEditModelValidator();

            var result = validator.Validate(vm);

            result.IsValid.Should().BeFalse();
            result.Errors.Count.Should().Be(1);
            result.Errors[0].PropertyName.Should().Be(nameof(SelectTrainingProviderEditModel.IsTrainingProviderSelected));
            result.Errors[0].ErrorMessage.Should().Be("Select yes if you’ve found a training provider");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ShouldErrorIfUkprnIsNotSpecified(string inputUkprn)
        {
            var vm = new SelectTrainingProviderEditModel {
                IsTrainingProviderSelected = true,
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
                IsTrainingProviderSelected = true,
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
                IsTrainingProviderSelected = true,
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
                IsTrainingProviderSelected = true,
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
                IsTrainingProviderSelected = true,
                TrainingProviderSearch = "something specified",
                SelectionType = TrainingProviderSelectionType.TrainingProviderSearch
            };

            var validator = new SelectTrainingProviderEditModelValidator();

            var result = validator.Validate(vm);

            result.IsValid.Should().BeTrue();
        }
    }
}
