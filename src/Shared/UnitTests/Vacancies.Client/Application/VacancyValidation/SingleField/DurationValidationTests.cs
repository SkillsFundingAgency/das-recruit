using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.UnitTests.Vacancies.Client.Application.VacancyValidation.SingleField
{
    public class DurationValidationTests : VacancyValidationTestsBase
    {
        [Fact]
        public void NoErrorsWhenDurationFieldsAreValid()
        {
            var vacancy = new Vacancy
            {
                Wage = new Wage
                {
                    DurationUnit = DurationUnit.Month,
                    Duration = 12
                }
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.Duration);

            result.HasErrors.Should().BeFalse();
            result.Errors.Should().HaveCount(0);
        }


        [Fact]
        public void DurationUnitMustHaveAValue()
        {
            var vacancy = new Vacancy
            {
                Wage = new Wage
                {
                    DurationUnit = null,
                    Duration = 13
                }
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.Duration);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Wage)}.{nameof(vacancy.Wage.DurationUnit)}");
            result.Errors[0].ErrorCode.Should().Be("34");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.Duration);
        }

        [Fact]
        public void DurationUnitMustHaveAValidValue()
        {
            var vacancy = new Vacancy
            {
                Wage = new Wage
                {
                    DurationUnit = (DurationUnit)1000,
                    Duration = 13
                }
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.Duration);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Wage)}.{nameof(vacancy.Wage.DurationUnit)}");
            result.Errors[0].ErrorCode.Should().Be("34");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.Duration);
        }

        [Fact]
        public void DurationMustHaveAValue()
        {
            var vacancy = new Vacancy 
            {
                Wage = new Wage
                {
                    DurationUnit = DurationUnit.Month,
                    Duration = null
                }
            };
            
            var result = Validator.Validate(vacancy, VacancyRuleSet.Duration);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Wage)}.{nameof(vacancy.Wage.Duration)}");
            result.Errors[0].ErrorCode.Should().Be("34");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.Duration);
        }

        [Theory]
        [InlineData(DurationUnit.Month, 11)]
        public void DurationMustBeAtLeast12Months(DurationUnit unitValue, int durationValue)
        {
            var vacancy = new Vacancy
            {
                Wage = new Wage
                {
                    DurationUnit = unitValue,
                    Duration = durationValue
                }
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.Duration);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Wage)}.{nameof(vacancy.Wage.Duration)}");
            result.Errors[0].ErrorCode.Should().Be("36");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.Duration);
        }
    }
}