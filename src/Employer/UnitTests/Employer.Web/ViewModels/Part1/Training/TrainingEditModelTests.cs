using Esfa.Recruit.Employer.Web.ViewModels;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.ViewModels.Part1.Training
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using Esfa.Recruit.Employer.Web.ViewModels.Part1.Training;
    using FluentAssertions;
    using Xunit;
    using ErrMsg = ValidationMessages.TrainingValidationMessages;

    public class TrainingEditModelTests
    {
        public static IEnumerable<object[]> InvalidTrainingData =>
            new List<object[]>
            {
                //ClosingDate
                new object[] { nameof(TrainingEditModel.ClosingDay), null, nameof(TrainingEditModel.ClosingDate), ErrMsg.Required.ClosingDate},
                new object[] { nameof(TrainingEditModel.ClosingDay), "aa", nameof(TrainingEditModel.ClosingDate), ErrMsg.TypeOfDate.ClosingDate},
                new object[] { nameof(TrainingEditModel.ClosingDay), "32", nameof(TrainingEditModel.ClosingDate), ErrMsg.TypeOfDate.ClosingDate},

                new object[] { nameof(TrainingEditModel.ClosingMonth), null, nameof(TrainingEditModel.ClosingDate), ErrMsg.Required.ClosingDate},
                new object[] { nameof(TrainingEditModel.ClosingMonth), "aa", nameof(TrainingEditModel.ClosingDate), ErrMsg.TypeOfDate.ClosingDate},
                new object[] { nameof(TrainingEditModel.ClosingMonth), "13", nameof(TrainingEditModel.ClosingDate), ErrMsg.TypeOfDate.ClosingDate},

                new object[] { nameof(TrainingEditModel.ClosingYear), null, nameof(TrainingEditModel.ClosingDate), ErrMsg.Required.ClosingDate},
                new object[] { nameof(TrainingEditModel.ClosingYear), "aa", nameof(TrainingEditModel.ClosingDate), ErrMsg.TypeOfDate.ClosingDate},

                //StartDate
                new object[] { nameof(TrainingEditModel.StartDay), null, nameof(TrainingEditModel.StartDate), ErrMsg.Required.StartDate},
                new object[] { nameof(TrainingEditModel.StartDay), "aa", nameof(TrainingEditModel.StartDate), ErrMsg.TypeOfDate.StartDate},
                new object[] { nameof(TrainingEditModel.StartDay), "32", nameof(TrainingEditModel.StartDate), ErrMsg.TypeOfDate.StartDate},

                new object[] { nameof(TrainingEditModel.StartMonth), null, nameof(TrainingEditModel.StartDate), ErrMsg.Required.StartDate},
                new object[] { nameof(TrainingEditModel.StartMonth), "aa", nameof(TrainingEditModel.StartDate), ErrMsg.TypeOfDate.StartDate},
                new object[] { nameof(TrainingEditModel.StartMonth), "13", nameof(TrainingEditModel.StartDate), ErrMsg.TypeOfDate.StartDate},

                new object[] { nameof(TrainingEditModel.StartYear), null, nameof(TrainingEditModel.StartDate), ErrMsg.Required.StartDate},
                new object[] { nameof(TrainingEditModel.StartYear), "aa", nameof(TrainingEditModel.StartDate), ErrMsg.TypeOfDate.StartDate},

                new object[] { nameof(TrainingEditModel.EmployerAccountId), null, nameof(TrainingEditModel.EmployerAccountId), "The EmployerAccountId field is required." },
                new object[] { nameof(TrainingEditModel.VacancyId), default(Guid), nameof(TrainingEditModel.VacancyId), "The field VacancyId is invalid." },
            };

        [Theory]
        [MemberData(nameof(InvalidTrainingData))]
        public void ShouldErrorIfClosingDateIsInvalid(string propertyName, object actualPropertyValue, string expectedErrorPropertyName, string expectedErrorMessage)
        {
            //a valid model
            var m = new TrainingEditModel
            {
                EmployerAccountId = "valid",
                VacancyId = Guid.Parse("53b54daa-4702-4b69-97e5-12123a59f8ad"),
                ClosingDay = "01",
                ClosingMonth = "3",
                ClosingYear = "18",
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
