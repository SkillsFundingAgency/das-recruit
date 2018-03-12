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
        public static IEnumerable<object[]> InvalidTitleData =>
            new List<object[]>
            {
                new object[] { null, "The Title field is required."},
                new object[] { new string('a', 101), "The field Title must be a string with a minimum length of 1 and a maximum length of 100." },
                new object[] { "<", "Title contains invalid characters." }
            };

        [Theory]
        [MemberData(nameof(InvalidTitleData))]
        public void ShouldErrorIfTitleIsInvalid(string actualTitle, string expectedErrorMessage)
        {
            var vm = new TitleEditModel
            {
                Title = actualTitle,
                EmployerAccountId = null
            };

            var context = new ValidationContext(vm, null, null);
            var result = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(vm, context, result, true);
            
            isValid.Should().BeFalse();
            result.Should().HaveCount(2);
            result.Single(r => r.MemberNames.Single() == "Title").ErrorMessage.Should().Be(expectedErrorMessage);
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
