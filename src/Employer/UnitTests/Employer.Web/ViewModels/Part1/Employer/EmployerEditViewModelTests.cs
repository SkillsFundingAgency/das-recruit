namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.ViewModels.Part1.Employer
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using Recruit.Employer.Web.ViewModels.Part1.Employer;
    using FluentAssertions;
    using Xunit;
    using ErrMsg = Esfa.Recruit.Employer.Web.ViewModels.ValidationMessages.EmployerEditModelValidationMessages;

    public class EmployerEditViewModelTests
    {
        public static IEnumerable<object[]> InvalidEmployerData =>
            new List<object[]>
            {
                new object[] { "AddressLine1", null, ErrMsg.Required.AddressLine1},
                new object[] { "AddressLine1", "<", ErrMsg.FreeText.AddressLine1},
                new object[] { "AddressLine1", new string('a', 101), string.Format(ErrMsg.StringLength.AddressLine1, "AddressLine1", 100)},

                new object[] { "AddressLine2", "<", ErrMsg.FreeText.AddressLine2},
                new object[] { "AddressLine2", new string('a', 101), string.Format(ErrMsg.StringLength.AddressLine2, "AddressLine2", 100) },

                new object[] { "AddressLine3", "<", ErrMsg.FreeText.AddressLine3},
                new object[] { "AddressLine3", new string('a', 101), string.Format(ErrMsg.StringLength.AddressLine3, "AddressLine3", 100) },

                new object[] { "AddressLine4", "<", ErrMsg.FreeText.AddressLine4},
                new object[] { "AddressLine4", new string('a', 101), string.Format(ErrMsg.StringLength.AddressLine4, "AddressLine4", 100) },

                new object[] { "Postcode", null, ErrMsg.Required.Postcode},
                new object[] { "Postcode", "<", ErrMsg.PostcodeAttribute.Postcode},
                new object[] { "Postcode", "SW1A 2AAA", ErrMsg.PostcodeAttribute.Postcode},

                new object[] { "SelectedOrganisationId", null, ErrMsg.Required.SelectedOrganisationId},

                new object[] { "EmployerAccountId", null, "The EmployerAccountId field is required."},
                new object[] { "VacancyId", default(Guid), "The field VacancyId is invalid."},
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
