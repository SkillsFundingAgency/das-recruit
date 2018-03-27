namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.ViewModels.Part1.Title
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using Esfa.Recruit.Employer.Web.ViewModels.Part1.Title;
    using FluentAssertions;
    using Xunit;
    using ErrMsg = Esfa.Recruit.Employer.Web.ViewModels.ValidationMessages.TitleValidationMessages;

    public class TitleEditModelTests
    {
        [Fact]
        public void ShouldErrorIfTitleEditModelIsInvalid()
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
            result.Should().HaveCount(1);
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
