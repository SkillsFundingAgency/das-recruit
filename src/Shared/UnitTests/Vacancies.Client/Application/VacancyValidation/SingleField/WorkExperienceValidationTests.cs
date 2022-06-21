using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.UnitTests.Vacancies.Client.Application.VacancyValidation.SingleField
{
    public class WorkExperienceValidationTests : VacancyValidationTestsBase
    {
        public WorkExperienceValidationTests()
        {
            ServiceParameters = new ServiceParameters("Traineeship");
        }
        [Fact]
        public void NoErrorsWhenWorkExperienceFieldIsValid()
        {
            var vacancy = new Vacancy 
            {
                WorkExperience = "a valid description"
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.WorkExperience);

            result.HasErrors.Should().BeFalse();
            result.Errors.Should().HaveCount(0);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void WorkExperienceMustNotBeEmpty(string workExperience)
        {
            var vacancy = new Vacancy 
            {
                WorkExperience = workExperience
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.WorkExperience);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.WorkExperience));
            result.Errors[0].ErrorCode.Should().Be("83");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.WorkExperience);
        }

        [Fact]
        public void TrainingDescriptionMustNotBeLongerThanMaxLength()
        {
            var vacancy = new Vacancy 
            {
                WorkExperience = new string('a', 4001)
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.WorkExperience);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.WorkExperience));
            result.Errors[0].ErrorCode.Should().Be("81");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.WorkExperience);
        }

        [Theory]
        [InlineData("<p><br></p><ul><li>item1</li><li>item2</li></ul>", true)]
        [InlineData("<script>alert('not allowed')</script>", false)]
        [InlineData("<p>`</p>", false)]
        public void TrainingDescriptionMustContainValidHtml(string actual, bool expectedResult)
        {
            var vacancy = new Vacancy
            {
                WorkExperience = actual
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.WorkExperience);

            if (expectedResult)
            {
                result.HasErrors.Should().BeFalse();
            }
            else
            {
                result.HasErrors.Should().BeTrue();
                result.Errors.Should().HaveCount(1);
                result.Errors[0].PropertyName.Should().Be(nameof(vacancy.WorkExperience));
                result.Errors[0].ErrorCode.Should().Be("82");
                result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.WorkExperience);
            }
        }

        [Theory]
        [InlineData("some text bother")]
        [InlineData("some text dang")]
        [InlineData("some text drat")]
        [InlineData("some text balderdash")]
        public void TrainingDescription_ShouldFailIfContainsWordsFromTheProfanityList(string freeText)
        {
            var vacancy = new Vacancy()
            {
                WorkExperience = freeText
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.WorkExperience);
            result.HasErrors.Should().BeTrue();
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.WorkExperience));
            result.Errors.Count.Should().Be(1);
            result.Errors[0].ErrorCode.Should().Be("615");
        }

        [Theory]
        [InlineData("some textbother")]
        [InlineData("some textdang")]
        [InlineData("some textdrat")]
        [InlineData("some textbalderdash")]
        public void TrainingDescription_Should_Not_FailIfContainsWordsFromTheProfanityList(string freeText)
        {
            var vacancy = new Vacancy()
            {
                WorkExperience = freeText
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.WorkExperience);
            result.HasErrors.Should().BeFalse();
        }
    }
}