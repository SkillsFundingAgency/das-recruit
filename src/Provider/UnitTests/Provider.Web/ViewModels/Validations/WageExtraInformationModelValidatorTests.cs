using Esfa.Recruit.Provider.Web.ViewModels.Part1.Wage;
using Esfa.Recruit.Provider.Web.ViewModels.Validations.Fluent;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.ViewModels.Validations
{
    public class WageExtraInformationModelValidatorTests
    {
        [Fact]
        public void ExtraInformation_WithinMaxCharacters_ShouldPassValidation()
        {
            var m = new WageExtraInformationViewModel
            {
                WageAdditionalInformation = "This is a sample information within  the word limit."
            };

            var validator = new WageExtraInformationModelValidator();

            var result = validator.Validate(m);

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void ExtraInformation_WithNonValidFreeTextCharacters_ShouldFailValidation()
        {
            var m = new WageExtraInformationViewModel
            {
                WageAdditionalInformation = "This is a sample information within <div> the word limit."
            };

            var validator = new WageExtraInformationModelValidator();

            var result = validator.Validate(m);

            result.IsValid.Should().BeFalse();
            result.Errors.Count.Should().Be(1);
            result.Errors[0].ErrorMessage.Should().Be(string.Format(WageExtraInformationModelValidator.WageAdditionalInformationFreeTextCharacters));
        }

        [Fact]
        public void ExtraInformation_ExceedsMaxCharacters_ShouldFailValidation()
        {
            var m = new WageExtraInformationViewModel
            {
                WageAdditionalInformation = new string('W', WageExtraInformationModelValidator.WageAdditionalInformationMaxLength + 1)
            };

            var validator = new WageExtraInformationModelValidator();

            var result = validator.Validate(m);

            result.IsValid.Should().BeFalse();
            result.Errors.Count.Should().Be(1);
            result.Errors[0].ErrorMessage.Should().Be(string.Format(WageExtraInformationModelValidator.WageAdditionalInformationLength));
        }
    }
}
