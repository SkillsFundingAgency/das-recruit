using Esfa.Recruit.Employer.Web.ViewModels;
using FluentAssertions;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Xunit;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.ViewModels
{
    public class SelectTrainingProviderEditModelTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ShouldErrorIfUkprnIsNotSpecified(string inputUkprn)
        {
            var vm = new SelectTrainingProviderEditModel
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
    }
}
