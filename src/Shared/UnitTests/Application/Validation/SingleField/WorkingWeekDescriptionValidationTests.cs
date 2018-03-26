using System;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Enums;
using Esfa.Recruit.Vacancies.Client.Domain.Services;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Application.Validation.SingleField
{
    public class WorkingWeekDescriptionValidationTests
    {
        private IEntityValidator<Vacancy, VacancyRuleSet> _validator;

        public WorkingWeekDescriptionValidationTests()
        {
            var timeProvider = new CurrentTimeProvider();

            _validator = new EntityValidator<Vacancy, VacancyRuleSet>(new FluentVacancyValidator(timeProvider));
        }

        [Fact]
        public void NoErrorsWhenWorkingWeekDescriptionValueIsValid()
        {
            var vacancy = new Vacancy
            {
                Wage = new Wage
                {
                    WorkingWeekDescription = "This is a valid value"
                }
            };

            var result = _validator.Validate(vacancy, VacancyRuleSet.WorkingWeekDescription);

            result.HasErrors.Should().BeFalse();
            result.Errors.Should().HaveCount(0);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void WorkingWeekDescriptionMustHaveAValue(string descriptionValue)
        {
            var vacancy = new Vacancy
            {
                Wage = new Wage
                {
                    WorkingWeekDescription = descriptionValue
                }
            };

            var result = _validator.Validate(vacancy, VacancyRuleSet.WorkingWeekDescription);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Wage)}.{nameof(vacancy.Wage.WorkingWeekDescription)}");
            result.Errors[0].ErrorCode.Should().Be("37");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.WorkingWeekDescription);
        }

        [Theory]
        [InlineData("<")]
        [InlineData(">")]
        public void WorkingWeekDescriptionMustContainValidCharacters(string invalidCharacter)
        {
            var vacancy = new Vacancy
            {
                Wage = new Wage
                {
                    WorkingWeekDescription = new String('a', 50) + invalidCharacter + new String('a', 50)
                }
            };

            var result = _validator.Validate(vacancy, VacancyRuleSet.WorkingWeekDescription);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Wage)}.{nameof(vacancy.Wage.WorkingWeekDescription)}");
            result.Errors[0].ErrorCode.Should().Be("38");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.WorkingWeekDescription);
        }

        [Fact]
        public void WorkingWeekDescriptionMustBeLessThan250Characters()
        {
            var vacancy = new Vacancy
            {
                Wage = new Wage
                {
                    WorkingWeekDescription = new string('a', 251)
                }
            };

            var result = _validator.Validate(vacancy, VacancyRuleSet.WorkingWeekDescription);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Wage)}.{nameof(vacancy.Wage.WorkingWeekDescription)}");
            result.Errors[0].ErrorCode.Should().Be("39");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.WorkingWeekDescription);
        }
    }
}