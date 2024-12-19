using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Duration;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using ErrMsg = Esfa.Recruit.Shared.Web.ViewModels.ValidationMessages.DurationValidationMessages;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.ViewModels.Part1.Duration
{
    public class DurationEditModelTests
    {
        [Test]
        public void Duration_ShouldErrorIfInvalid()
        {
            var m = GetDurationEditModel();
            m.Duration = "1.1";

            var context = new ValidationContext(m, null, null);
            var result = new List<ValidationResult>();
            
            var isValid = Validator.TryValidateObject(m, context, result, true);

            isValid.Should().BeFalse();
            result.Should().HaveCount(1);
            result[0].ErrorMessage.Should().Be(ErrMsg.TypeOfInteger.Duration);
        }

        [Test]
        public void WeeklyHours_ShouldErrorIfInvalid()
        {
            var m = GetDurationEditModel();
            m.WeeklyHours = "27.234";

            var context = new ValidationContext(m, null, null);
            var result = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(m, context, result, true);

            isValid.Should().BeFalse();
            result.Should().HaveCount(1);
            result[0].ErrorMessage.Should().Be(ErrMsg.TypeOfDecimal.WeeklyHours);
        }

        private DurationEditModel GetDurationEditModel()
        {
            return new DurationEditModel
            {
                EmployerAccountId = "valid",
                VacancyId = Guid.Parse("53b54daa-4702-4b69-97e5-12123a59f8ad"),
                Duration = "1",
                DurationUnit = DurationUnit.Year,
                WorkingWeekDescription = "valid",
                WeeklyHours = "35.25"
            };
        }
    }
}

