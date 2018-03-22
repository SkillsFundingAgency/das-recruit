using System;
using Esfa.Recruit.Vacancies.Client.Application.Exceptions;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Application.Validation
{
    public class PositionValidationTests
    {
        private IEntityValidator<Vacancy, VacancyRuleSet> _validator;

        public PositionValidationTests()
        {
            _validator = new EntityValidator<Vacancy, VacancyRuleSet>(new FluentVacancyValidator());
        }

        [Fact]
        public void ShouldNotThrowExceptionIfPostisionFieldsAreValid()
        {
            var vacancy = new Vacancy
            {
                NumberOfPositions = 2,
                ShortDescription = new string('a', 60)
            };

            Action act = () => _validator.ValidateAndThrow(vacancy, VacancyRuleSet.NumberOfPostions | VacancyRuleSet.ShortDescription);

            act.Should().NotThrow<EntityValidationException>();
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

            Action act = () => _validator.ValidateAndThrow(vacancy, VacancyRuleSet.NumberOfPostions);

            var ex = act.Should().Throw<EntityValidationException>();
            ex.Which.ValidationResult.HasErrors.Should().BeTrue();
            ex.Which.ValidationResult.Errors.Count.Should().Be(1);
            ex.Which.ValidationResult.Errors[0].PropertyName.Should().Be(nameof(vacancy.NumberOfPositions));
            ex.Which.ValidationResult.Errors[0].ErrorCode.Should().Be("10");
            ex.Which.ValidationResult.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.NumberOfPostions);
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

            Action act = () => _validator.ValidateAndThrow(vacancy, VacancyRuleSet.ShortDescription);

            var ex = act.Should().Throw<EntityValidationException>();
            ex.Which.ValidationResult.HasErrors.Should().BeTrue();
            ex.Which.ValidationResult.Errors.Count.Should().Be(1);
            ex.Which.ValidationResult.Errors[0].PropertyName.Should().Be(nameof(vacancy.ShortDescription));
            ex.Which.ValidationResult.Errors[0].ErrorCode.Should().Be("12");
            ex.Which.ValidationResult.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.ShortDescription);
        }

        [Fact]
        public void ShortDescriptionMustNotBeMoreThan350Characters()
        {
            var vacancy = new Vacancy
            {
                ShortDescription = new String('a', 351)
            };

            Action act = () => _validator.ValidateAndThrow(vacancy, VacancyRuleSet.ShortDescription);

            var ex = act.Should().Throw<EntityValidationException>();
            ex.Which.ValidationResult.HasErrors.Should().BeTrue();
            ex.Which.ValidationResult.Errors.Count.Should().Be(1);
            ex.Which.ValidationResult.Errors[0].PropertyName.Should().Be(nameof(vacancy.ShortDescription));
            ex.Which.ValidationResult.Errors[0].ErrorCode.Should().Be("13");
            ex.Which.ValidationResult.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.ShortDescription);
        }

        [Fact]
        public void ShortDescriptionMustNotBeLessThan50Characters()
        {
            var vacancy = new Vacancy
            {
                ShortDescription = new String('a', 49)
            };

            Action act = () => _validator.ValidateAndThrow(vacancy, VacancyRuleSet.ShortDescription);

            var ex = act.Should().Throw<EntityValidationException>();
            ex.Which.ValidationResult.HasErrors.Should().BeTrue();
            ex.Which.ValidationResult.Errors.Count.Should().Be(1);
            ex.Which.ValidationResult.Errors[0].PropertyName.Should().Be(nameof(vacancy.ShortDescription));
            ex.Which.ValidationResult.Errors[0].ErrorCode.Should().Be("14");
            ex.Which.ValidationResult.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.ShortDescription);
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

            Action act = () => _validator.ValidateAndThrow(vacancy, VacancyRuleSet.ShortDescription);

            var ex = act.Should().Throw<EntityValidationException>();
            ex.Which.ValidationResult.HasErrors.Should().BeTrue();
            ex.Which.ValidationResult.Errors.Count.Should().Be(1);
            ex.Which.ValidationResult.Errors[0].PropertyName.Should().Be(nameof(vacancy.ShortDescription));
            ex.Which.ValidationResult.Errors[0].ErrorCode.Should().Be("15");
            ex.Which.ValidationResult.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.ShortDescription);
        }
    }
}