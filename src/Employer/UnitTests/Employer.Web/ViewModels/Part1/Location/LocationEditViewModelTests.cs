using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Esfa.Recruit.Employer.Web.ViewModels.Location;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.ViewModels.Part1.Location
{
    public class LocationEditViewModelTests
    {
        [Fact]
        public void ShouldErrorIfLocationEditModelIsInvalid()
        {
            var m = new LocationEditModel
            {
                Postcode = null,
                EmployerAccountId = null
            };

            var context = new ValidationContext(m, null, null);
            var result = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(m, context, result, true);

            isValid.Should().BeFalse();
            result.Should().HaveCount(3);
            result.Single(r => r.MemberNames.Single() == "VacancyId").ErrorMessage.Should().Be("The field VacancyId is invalid.");
            result.Single(r => r.MemberNames.Single() == "Postcode").ErrorMessage.Should().Be("The Postcode field is required.");
            result.Single(r => r.MemberNames.Single() == "EmployerAccountId").ErrorMessage.Should().Be("The EmployerAccountId field is required.");
        }

        [Fact]
        public void ShouldErrorIfPostcodeInvalid()
        {
            var m = new LocationEditModel
            {
                Postcode = "invalid post code",
                EmployerAccountId = "employer account Id",
                VacancyId = Guid.NewGuid()
            };

            var context = new ValidationContext(m, null, null);
            var result = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(m, context, result, true);

            isValid.Should().BeFalse();
            result.Should().HaveCount(1);
            result.Single(r => r.MemberNames.Single() == "Postcode").ErrorMessage.Should().Be("The Postcode is not valid.");
        }
    }
}
