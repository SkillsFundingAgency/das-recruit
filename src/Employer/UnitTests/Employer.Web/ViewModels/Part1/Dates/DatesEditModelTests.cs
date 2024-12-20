using Esfa.Recruit.Employer.Web.ViewModels.Part1.Dates;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ErrMsg = Esfa.Recruit.Shared.Web.ViewModels.ValidationMessages.DateValidationMessages;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.ViewModels.Part1.Dates
{
    public class DatesEditModelTests
    {
        public static IEnumerable<object[]> InvalidDatesData =>
            new List<object[]>
            {
                //ClosingDate
                new object[] { nameof(DatesEditModel.ClosingDay), "aa", nameof(DatesEditModel.ClosingDate), ErrMsg.TypeOfDate.ClosingDate},
                new object[] { nameof(DatesEditModel.ClosingDay), "32", nameof(DatesEditModel.ClosingDate), ErrMsg.TypeOfDate.ClosingDate},

                new object[] { nameof(DatesEditModel.ClosingMonth), "aa", nameof(DatesEditModel.ClosingDate), ErrMsg.TypeOfDate.ClosingDate},
                new object[] { nameof(DatesEditModel.ClosingMonth), "13", nameof(DatesEditModel.ClosingDate), ErrMsg.TypeOfDate.ClosingDate},

                new object[] { nameof(DatesEditModel.ClosingYear), "aa", nameof(DatesEditModel.ClosingDate), ErrMsg.TypeOfDate.ClosingDate},
                new object[] { nameof(DatesEditModel.ClosingYear), "18", nameof(DatesEditModel.ClosingDate), ErrMsg.TypeOfDate.ClosingDate},

                //StartDate
                new object[] { nameof(DatesEditModel.StartDay), "aa", nameof(DatesEditModel.StartDate), ErrMsg.TypeOfDate.StartDate},
                new object[] { nameof(DatesEditModel.StartDay), "32", nameof(DatesEditModel.StartDate), ErrMsg.TypeOfDate.StartDate},

                new object[] { nameof(DatesEditModel.StartMonth), "aa", nameof(DatesEditModel.StartDate), ErrMsg.TypeOfDate.StartDate},
                new object[] { nameof(DatesEditModel.StartMonth), "13", nameof(DatesEditModel.StartDate), ErrMsg.TypeOfDate.StartDate},

                new object[] { nameof(DatesEditModel.StartYear), "aa", nameof(DatesEditModel.StartDate), ErrMsg.TypeOfDate.StartDate},
                new object[] { nameof(DatesEditModel.StartYear), "18", nameof(DatesEditModel.StartDate), ErrMsg.TypeOfDate.StartDate}
            };

        [Theory]
        [TestCaseSource(nameof(InvalidDatesData))]
        public void ShouldErrorIfClosingDateIsInvalid(string propertyName, object actualPropertyValue, string expectedErrorPropertyName, string expectedErrorMessage)
        {
            //a valid model
            var m = new DatesEditModel
            {
                EmployerAccountId = "valid",
                VacancyId = Guid.Parse("53b54daa-4702-4b69-97e5-12123a59f8ad"),
                ClosingDay = "01",
                ClosingMonth = "3",
                ClosingYear = "2018",
                StartDay = "7",
                StartMonth = "04",
                StartYear = "2018"
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
            result.Single(r => r.MemberNames.Single() == expectedErrorPropertyName).ErrorMessage.Should().Be(expectedErrorMessage);
        }
    }
}
