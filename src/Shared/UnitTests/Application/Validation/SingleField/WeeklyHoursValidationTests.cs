using System;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Services;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Application.Validation.SingleField
{
    public class WeeklyHoursValidationTests
    {
        private IEntityValidator<Vacancy, VacancyRuleSet> _validator;

        public WeeklyHoursValidationTests()
        {
            var timeProvider = new CurrentTimeProvider();

            _validator = new EntityValidator<Vacancy, VacancyRuleSet>(new FluentVacancyValidator(timeProvider));
        }

        [Fact]
        public void NoErrorsWhenWeeklyHoursIsValid()
        {
            var vacancy = new Vacancy
            {
                Wage = new Wage
                {
                    WeeklyHours = 30
                }
            };

            var result = _validator.Validate(vacancy, VacancyRuleSet.WeeklyHours);

            result.HasErrors.Should().BeFalse();
            result.Errors.Should().HaveCount(0);
        }

        [Fact]
        public void WeeklyHoursMustHaveAValue()
        {
            var vacancy = new Vacancy
            {
                Wage = new Wage
                {
                    WeeklyHours = null
                }
            };

            var result = _validator.Validate(vacancy, VacancyRuleSet.WeeklyHours);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Wage)}.{nameof(vacancy.Wage.WeeklyHours)}");
            result.Errors[0].ErrorCode.Should().Be("40");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.WeeklyHours);
        }

        [Fact]
        public void WeeklyHoursMustBeMoreThan16()
        {
            var vacancy = new Vacancy
            {
                Wage = new Wage
                {
                    WeeklyHours = 15
                }
            };

            var result = _validator.Validate(vacancy, VacancyRuleSet.WeeklyHours);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Wage)}.{nameof(vacancy.Wage.WeeklyHours)}");
            result.Errors[0].ErrorCode.Should().Be("42");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.WeeklyHours);
        }

        [Fact]
        public void WeeklyHoursMustBeLeeThan48()
        {
            var vacancy = new Vacancy
            {
                Wage = new Wage
                {
                    WeeklyHours = 49
                }
            };

            var result = _validator.Validate(vacancy, VacancyRuleSet.WeeklyHours);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Wage)}.{nameof(vacancy.Wage.WeeklyHours)}");
            result.Errors[0].ErrorCode.Should().Be("43");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.WeeklyHours);
        }
    }
}