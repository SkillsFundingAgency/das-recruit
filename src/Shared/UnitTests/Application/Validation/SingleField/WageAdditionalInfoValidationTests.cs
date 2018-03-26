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
    public class WageAdditionalInfoValidationTests
    {
        private IEntityValidator<Vacancy, VacancyRuleSet> _validator;

        public WageAdditionalInfoValidationTests()
        {
            var timeProvider = new CurrentTimeProvider();

            _validator = new EntityValidator<Vacancy, VacancyRuleSet>(new FluentVacancyValidator(timeProvider));
        }

        [Fact]
        public void NoErrorsWhenWageAdditionalInfoValueIsValid()
        {
            var vacancy = new Vacancy
            {
                Wage = new Wage
                {
                    WageAdditionalInformation = "This is a valid value"
                }
            };

            var result = _validator.Validate(vacancy, VacancyRuleSet.WageAdditionalInformation);

            result.HasErrors.Should().BeFalse();
            result.Errors.Should().HaveCount(0);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void WageAdditionalInfoIsOptional(string descriptionValue)
        {
            var vacancy = new Vacancy
            {
                Wage = new Wage
                {
                    WageAdditionalInformation = descriptionValue
                }
            };

            var result = _validator.Validate(vacancy, VacancyRuleSet.WageAdditionalInformation);

            result.HasErrors.Should().BeFalse();
            result.Errors.Should().HaveCount(0);
        }

        [Theory]
        [InlineData("<")]
        [InlineData(">")]
        public void WageAdditionalInfoMustContainValidCharacters(string invalidCharacter)
        {
            var vacancy = new Vacancy
            {
                Wage = new Wage
                {
                    WageAdditionalInformation = new String('a', 50) + invalidCharacter + new String('a', 50)
                }
            };

            var result = _validator.Validate(vacancy, VacancyRuleSet.WageAdditionalInformation);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Wage)}.{nameof(vacancy.Wage.WageAdditionalInformation)}");
            result.Errors[0].ErrorCode.Should().Be("45");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.WageAdditionalInformation);
        }

        [Fact]
        public void WageAdditionalInfoMustBeLessThan241Characters()
        {
            var vacancy = new Vacancy
            {
                Wage = new Wage
                {
                    WageAdditionalInformation = new string('a', 242)
                }
            };

            var result = _validator.Validate(vacancy, VacancyRuleSet.WageAdditionalInformation);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Wage)}.{nameof(vacancy.Wage.WageAdditionalInformation)}");
            result.Errors[0].ErrorCode.Should().Be("44");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.WageAdditionalInformation);
        }
    }
}