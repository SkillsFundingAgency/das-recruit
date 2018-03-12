using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Title;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.ViewModels.Part1.Title
{
    public class TitleEditModelTests
    {
        [Fact]
        public void ShouldErrorIfTitleIsNotSpecified()
        {
            var vm = new TitleEditModel
            {
                Title = null,
                EmployerAccountId = null
            };

            var context = new ValidationContext(vm, null, null);
            var result = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(vm, context, result, true);
            
            isValid.Should().BeFalse();
            result.Should().HaveCount(2);
            result.Single(r => r.MemberNames.Single() == "Title").ErrorMessage.Should().Be("The Title field is required.");
            result.Single(r => r.MemberNames.Single() == "EmployerAccountId").ErrorMessage.Should().Be("The EmployerAccountId field is required.");
        }

        [Fact]
        public void ShouldBeValidIfTitleIsSpecified()
        {
            var vm = new TitleEditModel
            {
                Title = "some text",
                EmployerAccountId = "scotty"
            };

            var context = new ValidationContext(vm, null, null);
            var result = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(vm, context, result, false);

            isValid.Should().BeTrue();
        }
    }
}
