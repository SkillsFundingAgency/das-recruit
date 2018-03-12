using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.ShortDescription;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.ViewModels.Part1.ShortDescription
{
    public class ShortDescriptionTests
    {
        [Fact]
        public void ShouldErrorIfShortDescriptionEditModelIsInvalid()
        {
            var m = new ShortDescriptionEditModel
            {
                NumberOfPositions = null,
                ShortDescription = null,
                EmployerAccountId = null
            };

            var context = new ValidationContext(m, null, null);
            var result = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(m, context, result, true);

            isValid.Should().BeFalse();
            result.Should().HaveCount(4);
            result.Single(r => r.MemberNames.Single() == "EmployerAccountId").ErrorMessage.Should().Be("The EmployerAccountId field is required.");
            result.Single(r => r.MemberNames.Single() == "VacancyId").ErrorMessage.Should().Be("The field VacancyId is invalid.");
            result.Single(r => r.MemberNames.Single() == "ShortDescription").ErrorMessage.Should().Be("The Short description field is required.");
            result.Single(r => r.MemberNames.Single() == "NumberOfPositions").ErrorMessage.Should().Be("The Number of positions field is required.");


        }
    }
}
