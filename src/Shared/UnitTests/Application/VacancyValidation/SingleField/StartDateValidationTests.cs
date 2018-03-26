using System;
using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using UnitTests.Application.VacancyValidation;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Application.VacancyValidation.SingleField
{
    public class StartDateValidationTests : VacancyValidationTestsBase
    {
        public static IEnumerable<object[]> InvalidStartDates =>
            new List<object[]>
            {
                new object[] { DateTime.UtcNow.Date },
                new object[] { DateTime.UtcNow },
                new object[] { DateTime.UtcNow.AddDays(-1) }
            };

        [Fact]
        public void NoErrorsWhenStartDateIsValid()
        {
            var vacancy = new Vacancy
            {
                StartDate = DateTime.UtcNow.AddDays(5)
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.StartDate);

            result.HasErrors.Should().BeFalse();
            result.Errors.Should().HaveCount(0);
        }

        [Fact]
        public void StartDateMustHaveAValue()
        {
            var vacancy = new Vacancy 
            {
                StartDate = null
            };
            
            var result = Validator.Validate(vacancy, VacancyRuleSet.StartDate);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.StartDate)}");
            result.Errors[0].ErrorCode.Should().Be("20");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.StartDate);
        }

        [Theory]
        [MemberData(nameof(InvalidStartDates))]
        public void StartDateMustBeGreaterThanToday(DateTime startDate)
        {
            var vacancy = new Vacancy
            {
                StartDate = startDate
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.StartDate);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.StartDate)}");
            result.Errors[0].ErrorCode.Should().Be("22");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.StartDate);
        }
    }
}