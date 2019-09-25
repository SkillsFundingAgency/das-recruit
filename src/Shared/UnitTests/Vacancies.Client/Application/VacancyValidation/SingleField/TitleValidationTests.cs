using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.UnitTests.Vacancies.Client.Application.VacancyValidation.SingleField
{
    public class ValidateVacancyTests : VacancyValidationTestsBase
    {
        public static IEnumerable<object[]> ValidTitles =>
            new List<object[]>
            {
                new object[] { $"apprentice {new string('a', 89)}" },
                new object[] { "apprentice" }
            };

        [Theory]
        [MemberData(nameof(ValidTitles))]
        public void NoErrorsWhenTitleFieldIsValid(string validTitle)
        {
            var vacancy = new Vacancy 
            {
                Title = validTitle
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.Title);

            result.HasErrors.Should().BeFalse();
            result.Errors.Should().HaveCount(0);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void TitleMustHaveAValue(string titleValue)
        {
            var vacancy = new Vacancy 
            {
                Title = titleValue
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.Title);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.Title));
            result.Errors[0].ErrorCode.Should().Be("1");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.Title);
        }

        [Theory]
        [InlineData("Apprentice mage")]
        [InlineData("Apprenticeship in sorcery")]
        [InlineData("Mage apprentice")]
        [InlineData("Witchcraft apprenticeship")]
        [InlineData("junior apprentice mage")]
        [InlineData("junior apprenticeship in sorcery")]
        public void NoErrorsWhenTitleContainsTheWordApprenticeOrApprenticeship(string testValue)
        {
            var vacancy = new Vacancy
            {
                Title = testValue
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.Title);

            result.HasErrors.Should().BeFalse();
        }

        [Theory]
        [InlineData("mage")]
        [InlineData("Apprenticeshipin sorcery")]
        [InlineData("Mage apprenticesip")]
        [InlineData("Witchcraft aprentice")]
        [InlineData("aprentice mage")]
        [InlineData("junior apprenteeship in sorcery")]
        public void TitleMustContainTheWordApprenticeOrApprenticeship(string testValue)
        {
            var vacancy = new Vacancy
            {
                Title = testValue
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.Title);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.Title));
            result.Errors[0].ErrorCode.Should().Be("200");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.Title);
        }

        [Fact]
        public void TitleBeLongerThan100Characters()
        {
            var vacancy = new Vacancy 
            {
                Title = $"apprentice {new string('a', 110)}"
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.Title);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.Title));
            result.Errors[0].ErrorCode.Should().Be("2");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.Title);
        }

        [Theory]
        [InlineData("apprentice<")]
        [InlineData("apprentice>")]
        public void TitleMustContainValidCharacters(string testValue)
        {
            var vacancy = new Vacancy 
            {
                Title = testValue
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.Title);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.Title));
            result.Errors[0].ErrorCode.Should().Be("3");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.Title);
        }

        [Fact]
        public void Title_ShouldFailIfContainsWordsFromTheProfanityList()
        {
            var vacancy = new Vacancy()
            {
                Title = "vacancy title dangleberry apprentice"
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.Title);
            result.HasErrors.Should().BeTrue();
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.Title));
            result.Errors.Count.Should().Be(1);
            result.Errors[0].ErrorCode.Should().Be("5");
        }
    }
}