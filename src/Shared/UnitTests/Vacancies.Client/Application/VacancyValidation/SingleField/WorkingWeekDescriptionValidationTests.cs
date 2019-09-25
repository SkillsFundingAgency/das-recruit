using System;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.UnitTests.Vacancies.Client.Application.VacancyValidation.SingleField
{
    public class WorkingWeekDescriptionValidationTests : VacancyValidationTestsBase
    {
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

            var result = Validator.Validate(vacancy, VacancyRuleSet.WorkingWeekDescription);

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

            var result = Validator.Validate(vacancy, VacancyRuleSet.WorkingWeekDescription);

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

            var result = Validator.Validate(vacancy, VacancyRuleSet.WorkingWeekDescription);

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

            var result = Validator.Validate(vacancy, VacancyRuleSet.WorkingWeekDescription);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Wage)}.{nameof(vacancy.Wage.WorkingWeekDescription)}");
            result.Errors[0].ErrorCode.Should().Be("39");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.WorkingWeekDescription);
        }

        [Fact]
        public void WorkingWeekDescription_ShouldFailIfContainsWordsFromTheProfanityList()
        {
            var vacancy = new Vacancy
            {
                Wage = new Wage
                {
                    WorkingWeekDescription = "a tomato can description for working week dangleberry"
                }
            };
            var result = Validator.Validate(vacancy, VacancyRuleSet.WorkingWeekDescription);
            result.HasErrors.Should().BeTrue();
            result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Wage)}.{nameof(vacancy.Wage.WorkingWeekDescription)}");
            result.Errors.Count.Should().Be(1);
            result.Errors[0].ErrorCode.Should().Be("5");
        }
    }
}