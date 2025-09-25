using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Wage;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using ErrMsg = Esfa.Recruit.Shared.Web.ViewModels.ValidationMessages.WageValidationMessages;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.ViewModels.Part1.Wage
{
    public class WageEditModelTests
    {
        [TestCase("aa", ErrMsg.TypeOfMoney.FixedWageYearlyAmount)]
        [TestCase("$15,000.01", ErrMsg.TypeOfMoney.FixedWageYearlyAmount)]
        [TestCase("15,000.0135", ErrMsg.TypeOfMoney.FixedWageYearlyAmount)]
        public void FixedWage_ShouldErrorIfInvalid(string fixedWageYearlyAmount, string expectedErrorMessage)
        {
            var m = new WageEditModel
            {
                EmployerAccountId = "valid",
                VacancyId = Guid.Parse("53b54daa-4702-4b69-97e5-12123a59f8ad"),
                WageType = WageType.FixedWage,
                FixedWageYearlyAmount = fixedWageYearlyAmount,
                WageAdditionalInformation = "valid"
            };
            
            var context = new ValidationContext(m, null, null);
            var result = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(m, context, result, true);

            isValid.Should().BeFalse();
            result.Should().HaveCount(1);
            result[0].ErrorMessage.Should().Be(expectedErrorMessage);
        }
    }
}

