using Esfa.Recruit.Employer.Web.ViewModels.TrainingProvider;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Xunit;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.ViewModels.TrainingProvider
{
    public class ConfirmEditModelTests
    {
        private const string ValidTestUkprn = "12345678";
        private const string ValidDummyString = "spinach";

        private readonly Guid _dummyVacancyGuid = Guid.NewGuid();

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ShouldErrorIfUkprnIsNotSpecified(string inputUkprn)
        {
            var vm = new ConfirmEditModel
            {
                VacancyId = _dummyVacancyGuid,
                Ukprn = inputUkprn
            };

            var context = new ValidationContext(vm, null, null);
            var result = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(vm, context, result, true);

            isValid.Should().BeFalse();
            result.Single(r => r.MemberNames.Single() == "Ukprn").ErrorMessage.Should().Be("The UKPRN field is required.");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ShouldErrorIfProviderNameIsNotSpecified(string invalidProviderName)
        {
            var vm = new ConfirmEditModel
            {
                VacancyId = _dummyVacancyGuid,
                Ukprn = ValidTestUkprn,
                ProviderName = invalidProviderName,
                ProviderAddress = ValidDummyString
            };

            var context = new ValidationContext(vm, null, null);
            var result = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(vm, context, result, true);

            isValid.Should().BeFalse();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ShouldErrorIfProviderAddressIsNotSpecified(string invalidProviderAddress)
        {
            var vm = new ConfirmEditModel
            {
                VacancyId = _dummyVacancyGuid,
                Ukprn = ValidTestUkprn,
                ProviderName = ValidDummyString,
                ProviderAddress = invalidProviderAddress
            };

            var context = new ValidationContext(vm, null, null);
            var result = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(vm, context, result, true);

            isValid.Should().BeFalse();
        }

        [Theory]
        [InlineData("0")]
        [InlineData("1")]
        [InlineData("scott")]
        [InlineData("ozzyscott")]
        [InlineData("12345")]
        [InlineData("01234567")]
        public void ShouldErrorIfUkprnSpecifiedHasInvalidFormat(string inputUkprn)
        {
            var vm = new ConfirmEditModel
            {
                VacancyId = _dummyVacancyGuid,
                Ukprn = inputUkprn,
                ProviderName = ValidDummyString,
                ProviderAddress = ValidDummyString
            };

            var context = new ValidationContext(vm, null, null);
            var result = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(vm, context, result, true);

            isValid.Should().BeFalse();
            result.Single(r => r.MemberNames.Single() == "Ukprn").ErrorMessage.Should().Be("No provider found with this UK Provider Reference Number.");
        }

        [Fact]
        public void ShouldBeValidIfUkprnSpecified()
        {
            var vm = new ConfirmEditModel
            {
                VacancyId = _dummyVacancyGuid,
                Ukprn = "12345678",
                ProviderName = ValidDummyString,
                ProviderAddress = ValidDummyString
            };

            var context = new ValidationContext(vm, null, null);
            var result = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(vm, context, result, true);

            isValid.Should().BeTrue();
        }
    }
}
