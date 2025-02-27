using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.VacancyValidation.SingleField
{
    public class WorkingWeekDescriptionValidationTests : VacancyValidationTestsBase
    {
        [Fact]
        public void NoErrorsWhenWorkingWeekDescriptionValueIsValid()
        {
            ServiceParameters = new ServiceParameters();
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
        [InlineData( null)]
        [InlineData("")]
        public void WorkingWeekDescriptionMustHaveAValue(string descriptionValue)
        {
            ServiceParameters = new ServiceParameters();
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
        [InlineData( "<")]
        [InlineData( ">")]
        public void WorkingWeekDescriptionMustContainValidCharacters(string invalidCharacter)
        {
            ServiceParameters = new ServiceParameters();
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
            ServiceParameters = new ServiceParameters();
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
        [InlineData("some text bother")]
        [InlineData( "some text dang")]
        [InlineData( "some text drat")]
        [InlineData( "some text balderdash")]
        public void WorkingWeekDescription_ShouldFailIfContainsWordsFromTheProfanityList(string freeText)
        {
            ServiceParameters = new ServiceParameters();
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
        [InlineData("some textbother")]
        [InlineData( "some textdang")]
        [InlineData( "some textdrat")]
        [InlineData( "some textbalderdash")]
        public void WorkingWeekDescription_ShouldFail_Not_IfContainsWordsFromTheProfanityList(string freeText)
        {
            ServiceParameters = new ServiceParameters();
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