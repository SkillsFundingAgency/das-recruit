using System;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Services;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Application.Validation
{
    public class PositionValidationTests
    {
        private IEntityValidator<Vacancy, VacancyRuleSet> _validator;

        public PositionValidationTests()
        {
            var timeProvider = new CurrentTimeProvider();

            _validator = new EntityValidator<Vacancy, VacancyRuleSet>(new FluentVacancyValidator(timeProvider));
        }

        [Fact]
        public void NoErrorsWhenPositionFieldsAreValid()
        {
            var vacancy = new Vacancy
            {
                NumberOfPositions = 2,
                ShortDescription = new string('a', 60)
            };

            var result = _validator.Validate(vacancy, VacancyRuleSet.NumberOfPostions | VacancyRuleSet.ShortDescription);

            result.HasErrors.Should().BeFalse();
            result.Errors.Should().HaveCount(0);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(0)]
        public void NumberOfPositionMustHaveAValue(int? numOfPositionsValue)
        {
            var vacancy = new Vacancy 
            {
                NumberOfPositions = numOfPositionsValue
            };

            var result = _validator.Validate(vacancy, VacancyRuleSet.NumberOfPostions);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.NumberOfPositions));
            result.Errors[0].ErrorCode.Should().Be("10");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.NumberOfPostions);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ShortDescriptionMustHaveAValue(string shortDescriptionValue)
        {
            var vacancy = new Vacancy
            {
                ShortDescription = shortDescriptionValue
            };

            var result = _validator.Validate(vacancy, VacancyRuleSet.ShortDescription);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.ShortDescription));
            result.Errors[0].ErrorCode.Should().Be("12");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.ShortDescription);
        }

        [Fact]
        public void ShortDescriptionMustNotBeMoreThan350Characters()
        {
            var vacancy = new Vacancy
            {
                ShortDescription = new String('a', 351)
            };

            var result = _validator.Validate(vacancy, VacancyRuleSet.ShortDescription);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.ShortDescription));
            result.Errors[0].ErrorCode.Should().Be("13");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.ShortDescription);
        }

        [Fact]
        public void ShortDescriptionMustNotBeLessThan50Characters()
        {
            var vacancy = new Vacancy
            {
                ShortDescription = new String('a', 49)
            };

            var result = _validator.Validate(vacancy, VacancyRuleSet.ShortDescription);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.ShortDescription));
            result.Errors[0].ErrorCode.Should().Be("14");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.ShortDescription);
        }


        [Theory]
        [InlineData("<")]
        [InlineData(">")]
        public void ShortDescriptionMustContainValidCharacters(string invalidCharacter)
        {
            var vacancy = new Vacancy
            {
                ShortDescription = new String('a', 30) + invalidCharacter + new String('b', 30)
            };

            var result = _validator.Validate(vacancy, VacancyRuleSet.ShortDescription);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.ShortDescription));
            result.Errors[0].ErrorCode.Should().Be("15");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.ShortDescription);
        }
    }
}