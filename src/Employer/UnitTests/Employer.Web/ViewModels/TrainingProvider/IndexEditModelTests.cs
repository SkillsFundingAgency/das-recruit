using Esfa.Recruit.Employer.Web.ViewModels.TrainingProvider;
using FluentAssertions;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Xunit;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.ViewModels.TrainingProvider
{
    public class IndexEditModelTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ShouldErrorIfUkprnIsNotSpecified(string inputUkprn)
        {
            var vm = new IndexEditModel
            {
                Ukprn = inputUkprn
            };

            var context = new ValidationContext(vm, null, null);
            var result = new List<ValidationResult>();
            
            var isValid = Validator.TryValidateObject(vm, context, result, true);

            isValid.Should().BeFalse();
            result.Should().HaveCount(1);
            result.Single(r => r.MemberNames.Single() == "Ukprn").ErrorMessage.Should().Be("The UKPRN field is required.");
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
            var vm = new IndexEditModel
            {
                Ukprn = inputUkprn
            };

            var context = new ValidationContext(vm, null, null);
            var result = new List<ValidationResult>();
            
            var isValid = Validator.TryValidateObject(vm, context, result, true);

            isValid.Should().BeFalse();
            result.Should().HaveCount(1);
            result.Single(r => r.MemberNames.Single() == "Ukprn").ErrorMessage.Should().Be("No provider found with this UK Provider Reference Number.");
        }

        [Fact]
        public void ShouldBeValidIfUkprnSpecified()
        {
            var vm = new IndexEditModel
            {
                Ukprn = "12345678"
            };

            var context = new ValidationContext(vm, null, null);
            var result = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(vm, context, result, true);

            isValid.Should().BeTrue();
        }
    }
}
