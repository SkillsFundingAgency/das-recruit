using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Employer;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.ViewModels.Part1.Location
{
    public class EmployerEditViewModelTests
    {
        public static IEnumerable<object[]> InvalidEmployerData =>
            new List<object[]>
            {
                new object[] { "AddressLine1", null, "The Address Line 1 field is required."},
                new object[] { "AddressLine1", "<", "Address Line 1 contains invalid characters."},
                new object[] { "AddressLine1", new string('a', 101), "The field Address Line 1 must be a string with a maximum length of 100."},

                new object[] { "AddressLine2", "<", "Address Line 2 contains invalid characters."},
                new object[] { "AddressLine2", new string('a', 101), "The field Address Line 2 must be a string with a maximum length of 100."},

                new object[] { "AddressLine3", "<", "Address Line 3 contains invalid characters."},
                new object[] { "AddressLine3", new string('a', 101), "The field Address Line 3 must be a string with a maximum length of 100."},

                new object[] { "AddressLine4", "<", "Address Line 4 contains invalid characters."},
                new object[] { "AddressLine4", new string('a', 101), "The field Address Line 4 must be a string with a maximum length of 100."},

                new object[] { "Postcode", null, "The Postcode field is required."},
                new object[] { "Postcode", "<", "Postcode is not a valid postcode."},
                new object[] { "Postcode", "SW1A 2AAA", "Postcode is not a valid postcode."},

                new object[] { "SelectedOrganisationId", null, "You must select a legal entity."},

                new object[] { "EmployerAccountId", null, "The EmployerAccountId field is required."},
                new object[] { "VacancyId", new Guid(), "The field VacancyId is invalid."},
            };

        [Theory]
        [MemberData(nameof(InvalidEmployerData))]
        public void ShouldErrorIfEmployerEditModelAddressIsInvalid(string propertyName, object actualPropertyValue, string expectedErrorMessage)
        {
            //a valid model
            var m = new EmployerEditModel
            {
                EmployerAccountId = "valid",
                VacancyId = Guid.Parse("53b54daa-4702-4b69-97e5-12123a59f8ad"),
                SelectedOrganisationId = "valid",
                AddressLine1 = "valid",
                Postcode = "SW1A 2AA"
            };
            
            var context = new ValidationContext(m, null, null);
            var result = new List<ValidationResult>();

            //ensure we are starting with a valid model
            var isInitiallyValid = Validator.TryValidateObject(m, context, result, true);
            isInitiallyValid.Should().BeTrue();

            //set the property we are testing
            m.GetType().GetProperty(propertyName).SetValue(m, actualPropertyValue);

            // Act
            var isValid = Validator.TryValidateObject(m, context, result, true);

            isValid.Should().BeFalse();
            result.Should().HaveCount(1);
            result.Single(r => r.MemberNames.Single() == propertyName).ErrorMessage.Should().Be(expectedErrorMessage);
        }
        
    }
}
