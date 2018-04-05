﻿using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Application.VacancyValidation.SingleField
{
    public class SkillsTests : VacancyValidationTestsBase
    {
        [Fact]
        public void NoErrorsWhenSkillsAreValid()
        {
            var vacancy = new Vacancy
            {
                Skills = new List<string>
                {
                    new string('a', 100)
                }
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.Skills);

            result.HasErrors.Should().BeFalse();
            result.Errors.Should().HaveCount(0);
        }

        public static IEnumerable<object[]> NullOrZeroSkillCollection =>
            new List<object[]>
            {
                new object[] {null},
                new object[] {new List<string>()},
            };

        [Theory]
        [MemberData(nameof(NullOrZeroSkillCollection))]
        public void SkillsCollectionMustNotBeNullOrHaveZeroCount(List<string> skills)
        {
            var vacancy = new Vacancy
            {
                Skills = skills
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.Skills);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.Skills));
            result.Errors[0].ErrorCode.Should().Be("51");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.Skills);
        }

        public static IEnumerable<object[]> NullOrZeroSkill =>
            new List<object[]>
            {
                new object[] {new List<string>{null}},
                new object[] {new List<string>{string.Empty}},
            };

        [Theory]
        [MemberData(nameof(NullOrZeroSkill))]
        public void SkillMustNotBeEmpty(List<string> skills)
        {
            var vacancy = new Vacancy
            {
                Skills = skills
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.Skills);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Skills)}[0]");
            result.Errors[0].ErrorCode.Should().Be("51");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.Skills);
        }

        [Fact]
        public void SkillsMustNotContainInvalidCharacters()
        {
            var vacancy = new Vacancy
            {
                Skills = new List<string>
                {
                    "<"
                }
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.Skills);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Skills)}[0]");
            result.Errors[0].ErrorCode.Should().Be("6");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.Skills);
        }

        [Fact]
        public void SkillsMustNotBeGreaterThanMaxLength()
        {
            var vacancy = new Vacancy
            {
                Skills = new List<string>
                {
                    new string('a', 101)
                }
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.Skills);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Skills)}[0]");
            result.Errors[0].ErrorCode.Should().Be("7");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.Skills);
        }
    }
}
