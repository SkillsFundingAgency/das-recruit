using System;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.UnitTests.Vacancies.Client.Application.VacancyValidation.SingleField
{
    public class TrainingDescriptionValidationTests : VacancyValidationTestsBase
    {
        [Fact]
        public void NoErrorsWhenTrainingDescriptionFieldIsValid()
        {
            var vacancy = new Vacancy 
            {
                TrainingDescription = "a valid description"
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.TrainingDescription);

            result.HasErrors.Should().BeFalse();
            result.Errors.Should().HaveCount(0);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void TrainingDescriptionMustNotBeEmpty(string trainingDescription)
        {
            var vacancy = new Vacancy 
            {
                TrainingDescription = trainingDescription
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.TrainingDescription);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.TrainingDescription));
            result.Errors[0].ErrorCode.Should().Be("54");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.TrainingDescription);
        }

        [Fact]
        public void TrainingDescriptionMustNotBeLongerThanMaxLength()
        {
            var vacancy = new Vacancy 
            {
                TrainingDescription = new String('a', 1001)
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.TrainingDescription);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.TrainingDescription));
            result.Errors[0].ErrorCode.Should().Be("7");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.TrainingDescription);
        }

        [Theory]
        [InlineData("<p><br></p><ul><li>item1</li><li>item2</li></ul>", true)]
        [InlineData("<script>alert('not allowed')</script>", false)]
        [InlineData("<p>`</p>", false)]
        public void TrainingDescriptionMustContainValidHtml(string actual, bool expectedResult)
        {
            var vacancy = new Vacancy
            {
                TrainingDescription = actual
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.TrainingDescription);

            if (expectedResult)
            {
                result.HasErrors.Should().BeFalse();
            }
            else
            {
                result.HasErrors.Should().BeTrue();
                result.Errors.Should().HaveCount(1);
                result.Errors[0].PropertyName.Should().Be(nameof(vacancy.TrainingDescription));
                result.Errors[0].ErrorCode.Should().Be("6");
                result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.TrainingDescription);
            }
        }
    }
}