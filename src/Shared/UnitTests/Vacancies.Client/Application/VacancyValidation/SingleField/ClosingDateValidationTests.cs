using System;
using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.UnitTests.Vacancies.Client.Application.VacancyValidation.SingleField
{
    public class ClosingDateValidationTests : VacancyValidationTestsBase
    {
        public static IEnumerable<object[]> InvalidClosingDates =>
            new List<object[]>
            {
                new object[] { DateTime.UtcNow.Date },
                new object[] { DateTime.UtcNow },
                new object[] { DateTime.UtcNow.AddDays(13) }
            };


        [Fact]
        public void NoErrorsWhenClosingDateIsValid()
        {
            var vacancy = new Vacancy
            {
                ClosingDate = DateTime.UtcNow.AddDays(15)
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.ClosingDate);

            result.HasErrors.Should().BeFalse();
            result.Errors.Should().HaveCount(0);
        }

        [Fact]
        public void ClosingDateMustHaveAValue()
        {
            var vacancy = new Vacancy 
            {
                ClosingDate = null
            };
            
            var result = Validator.Validate(vacancy, VacancyRuleSet.ClosingDate);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.ClosingDate)}");
            result.Errors[0].ErrorCode.Should().Be("16");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.ClosingDate);
        }

        [Theory]
        [MemberData(nameof(InvalidClosingDates))]
        public void ClosingDateMustBeGreaterThanToday(DateTime closingDateValue)
        {
            var vacancy = new Vacancy
            {
                ClosingDate = closingDateValue
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.ClosingDate);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.ClosingDate)}");
            result.Errors[0].ErrorCode.Should().Be("18");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.ClosingDate);
        }
    }
}