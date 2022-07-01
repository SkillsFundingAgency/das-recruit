using System;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.UnitTests.Vacancies.Client.Application.VacancyValidation.SingleField
{
    public class WorkingWeekDescriptionValidationTests : VacancyValidationTestsBase
    {
        [Theory]
        [InlineData("Apprenticeship")]
        [InlineData("Traineeship")]
        public void NoErrorsWhenWorkingWeekDescriptionValueIsValid(string serviceType)
        {
            ServiceParameters = new ServiceParameters(serviceType);
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
        [InlineData("Apprenticeship", null)]
        [InlineData("Apprenticeship", "")]
        [InlineData("Traineeship", null)]
        [InlineData("Traineeship", "")]
        public void WorkingWeekDescriptionMustHaveAValue(string serviceType, string descriptionValue)
        {
            ServiceParameters = new ServiceParameters(serviceType);
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
        [InlineData("Apprenticeship", "<")]
        [InlineData("Apprenticeship", ">")]
        [InlineData("Traineeship", "<")]
        [InlineData("Traineeship", ">")]
        public void WorkingWeekDescriptionMustContainValidCharacters(string serviceType, string invalidCharacter)
        {
            ServiceParameters = new ServiceParameters(serviceType);
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

        [Theory]
        [InlineData("Apprenticeship")]
        [InlineData("Traineeship")]
        public void WorkingWeekDescriptionMustBeLessThan250Characters(string serviceType)
        {
            ServiceParameters = new ServiceParameters(serviceType);
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

        [Theory]
        [InlineData("Apprenticeship", "some text bother")]
        [InlineData("Apprenticeship", "some text dang")]
        [InlineData("Apprenticeship", "some text drat")]
        [InlineData("Apprenticeship", "some text balderdash")]
        [InlineData("Traineeship", "some text bother")]
        [InlineData("Traineeship", "some text dang")]
        [InlineData("Traineeship", "some text drat")]
        [InlineData("Traineeship", "some text balderdash")]
        public void WorkingWeekDescription_ShouldFailIfContainsWordsFromTheProfanityList(string serviceType, string freeText)
        {
            ServiceParameters = new ServiceParameters(serviceType);
            var vacancy = new Vacancy
            {
                Wage = new Wage
                {
                    WorkingWeekDescription = freeText
                }
            };
            var result = Validator.Validate(vacancy, VacancyRuleSet.WorkingWeekDescription);
            result.HasErrors.Should().BeTrue();
            result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Wage)}.{nameof(vacancy.Wage.WorkingWeekDescription)}");
            result.Errors.Count.Should().Be(1);
            result.Errors[0].ErrorCode.Should().Be("606");
        }

        [Theory]
        [InlineData("Apprenticeship", "some textbother")]
        [InlineData("Apprenticeship", "some textdang")]
        [InlineData("Apprenticeship", "some textdrat")]
        [InlineData("Apprenticeship", "some textbalderdash")]
        [InlineData("Traineeship", "some textbother")]
        [InlineData("Traineeship", "some textdang")]
        [InlineData("Traineeship", "some textdrat")]
        [InlineData("Traineeship", "some textbalderdash")]
        public void WorkingWeekDescription_ShouldFail_Not_IfContainsWordsFromTheProfanityList(string serviceType, string freeText)
        {
            ServiceParameters = new ServiceParameters(serviceType);
            var vacancy = new Vacancy
            {
                Wage = new Wage
                {
                    WorkingWeekDescription = freeText
                }
            };
            var result = Validator.Validate(vacancy, VacancyRuleSet.WorkingWeekDescription);
            result.HasErrors.Should().BeFalse();
        }
    }
}