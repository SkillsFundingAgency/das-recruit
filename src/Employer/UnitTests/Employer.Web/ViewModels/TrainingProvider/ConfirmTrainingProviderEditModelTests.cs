using Esfa.Recruit.Employer.Web.ViewModels;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Xunit;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.ViewModels
{
    public class ConfirmTrainingProviderEditModelTests
    {
        private readonly Guid _dummyVacancyGuid = Guid.NewGuid();

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ShouldErrorIfUkprnIsNotSpecified(string inputUkprn)
        {
            var vm = new ConfirmTrainingProviderEditModel
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

        [Fact]
        public void ShouldBeValidIfUkprnSpecified()
        {
            var vm = new ConfirmTrainingProviderEditModel
            {
                VacancyId = _dummyVacancyGuid,
                Ukprn = "12345678"
            };

            var context = new ValidationContext(vm, null, null);
            var result = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(vm, context, result, true);

            isValid.Should().BeTrue();
        }
    }
}
