using System;
using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Services;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Application.Validation.SingleField
{
    public class ClosingDateValidationTests
    {
        private IEntityValidator<Vacancy, VacancyRuleSet> _validator;

        public ClosingDateValidationTests()
        {
            var timeProvider = new CurrentTimeProvider();

            _validator = new EntityValidator<Vacancy, VacancyRuleSet>(new FluentVacancyValidator(timeProvider));
        }

        public static IEnumerable<object[]> InvalidClosingDates =>
            new List<object[]>
            {
                new object[] { DateTime.UtcNow.Date },
                new object[] { DateTime.UtcNow },
                new object[] { DateTime.UtcNow.AddDays(-1) }
            };


        [Fact]
        public void NoErrorsWhenClosingDateIsValid()
        {
            var vacancy = new Vacancy
            {
                ClosingDate = DateTime.UtcNow.AddDays(5)
            };

            var result = _validator.Validate(vacancy, VacancyRuleSet.ClosingDate);

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
            
            var result = _validator.Validate(vacancy, VacancyRuleSet.ClosingDate);

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

            var result = _validator.Validate(vacancy, VacancyRuleSet.ClosingDate);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.ClosingDate)}");
            result.Errors[0].ErrorCode.Should().Be("18");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.ClosingDate);
        }
    }
}