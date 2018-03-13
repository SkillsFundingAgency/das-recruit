namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.ViewModels.Part1.Title
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using Esfa.Recruit.Employer.Web.ViewModels.Part1.Title;
    using FluentAssertions;
    using Xunit;
    using ErrMsg = Esfa.Recruit.Employer.Web.ViewModels.ValidationMessages.TitleValidationMessages;

    public class TitleEditModelTests
    {
        public static IEnumerable<object[]> InvalidTitleData =>
            new List<object[]>
            {
                new object[] { null, ErrMsg.Required.Title},
                new object[] { new string('a', 101), string.Format(ErrMsg.StringLength.Title, "Title", 100)},
                new object[] { "<", ErrMsg.FreeText.Title }
            };

        [Theory]
        [MemberData(nameof(InvalidTitleData))]
        public void ShouldErrorIfTitleEditModelIsInvalid(string actualTitle, string expectedErrorMessage)
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
        public void ShouldBeValidIfTitleEditModelIsValid()
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
