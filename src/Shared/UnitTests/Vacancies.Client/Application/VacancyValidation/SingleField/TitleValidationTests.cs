using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
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
        public static IEnumerable<object[]> ValidTraineeshipTitles =>
            new List<object[]>
            {
                new object[] { $"trainee {new string('a', 89)}" },
                new object[] { "traineeship" }
            };

        [Theory]
        [MemberData(nameof(ValidTitles))]
        public void NoErrorsWhenTitleFieldIsValidForApprenticeship(string validTitle)
        {
            ServiceParameters = new ServiceParameters("Apprenticeship");
            var vacancy = new Vacancy 
            {
                Title = validTitle
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.Title);

            result.HasErrors.Should().BeFalse();
            result.Errors.Should().HaveCount(0);
        }
        [Theory]
        [MemberData(nameof(ValidTraineeshipTitles))]
        public void NoErrorsWhenTitleFieldIsValidForTraineeship(string validTitle)
        {
            ServiceParameters = new ServiceParameters("Traineeship");
            var vacancy = new Vacancy 
            {
                Title = validTitle
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.Title);

            result.HasErrors.Should().BeFalse();
            result.Errors.Should().HaveCount(0);
        }

        [Theory]
        [InlineData(null, "Apprenticeship")]
        [InlineData("", "Apprenticeship")]
        [InlineData(null, "Traineeship")]
        [InlineData("", "Traineeship")]
        public void TitleMustHaveAValue(string titleValue, string serviceType)
        {
            ServiceParameters = new ServiceParameters(serviceType);
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
            ServiceParameters = new ServiceParameters("Apprenticeship");
            var vacancy = new Vacancy
            {
                Title = testValue
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.Title);

            result.HasErrors.Should().BeFalse();
        }
        
        [Theory]
        [InlineData("Trainee mage")]
        [InlineData("Traineeship in sorcery")]
        [InlineData("Mage trainee")]
        [InlineData("Witchcraft traineeship")]
        [InlineData("junior trainee mage")]
        [InlineData("junior traineeship in sorcery")]
        public void NoErrorsWhenTitleContainsTheWordTraineeOrTraineeship(string testValue)
        {
            ServiceParameters = new ServiceParameters("Traineeship");
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
            ServiceParameters = new ServiceParameters("Apprenticeship");
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
        
        [Theory]
        [InlineData("mage")]
        [InlineData("Traineeshipin sorcery")]
        [InlineData("Mage trainesip")]
        [InlineData("Witchcraft traine")]
        [InlineData("Traine mage")]
        [InlineData("junior traineship in sorcery")]
        public void TitleMustContainTheWordTraineeOrTraineeship(string testValue)
        {
            ServiceParameters = new ServiceParameters("Traineeship");
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

        [Theory]
        [InlineData("Apprenticeship")]
        [InlineData("Traineeship")]
        public void TitleBeLongerThan100Characters(string serviceType)
        {
            ServiceParameters = new ServiceParameters(serviceType);
            var vacancy = new Vacancy 
            {
                Title = $"apprentice {new string('a', 101)}"
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.Title);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.Title));
            result.Errors[0].ErrorCode.Should().Be("2");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.Title);
        }

        [Theory]
        [InlineData("apprentice<", "Apprenticeship")]
        [InlineData("apprentice>", "Apprenticeship")]
        [InlineData("trainee<", "Traineeship")]
        [InlineData("trainee>", "Traineeship")]
        public void TitleMustContainValidCharacters(string testValue, string serviceType)
        {
            ServiceParameters = new ServiceParameters(serviceType);
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

        [Theory]
        [InlineData("some text bother apprentice", "Apprenticeship")]
        [InlineData("some text dang apprentice", "Apprenticeship")]
        [InlineData("some text drat apprentice", "Apprenticeship")]
        [InlineData("some text balderdash apprentice", "Apprenticeship")]
        [InlineData("some text bother trainee", "Traineeship")]
        [InlineData("some text dang trainee", "Traineeship")]
        [InlineData("some text drat trainee", "Traineeship")]
        [InlineData("some text balderdash trainee", "Traineeship")]
        public void Title_ShouldFailIfContainsWordsFromTheProfanityList(string freeText, string serviceType)
        {
            ServiceParameters = new ServiceParameters(serviceType);
            var vacancy = new Vacancy()
            {
                Title = freeText
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.Title);
            result.HasErrors.Should().BeTrue();
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.Title));
            result.Errors.Count.Should().Be(1);
            result.Errors[0].ErrorCode.Should().Be("601");
        }

        [Theory]
        [InlineData("some textbother apprentice", "Apprenticeship")]
        [InlineData("some textdang apprentice", "Apprenticeship")]
        [InlineData("some textdrat apprentice", "Apprenticeship")]
        [InlineData("some textbalderdash apprentice", "Apprenticeship")]
        [InlineData("some textbother trainee", "Traineeship")]
        [InlineData("some textdang trainee", "Traineeship")]
        [InlineData("some textdrat trainee", "Traineeship")]
        [InlineData("some textbalderdash trainee", "Traineeship")]
        public void Title_Should_Not_Fail_IfWordContainsWordsFromTheProfanityList(string freeText, string serviceType)
        {
            ServiceParameters = new ServiceParameters(serviceType);
            var vacancy = new Vacancy()
            {
                Title = freeText
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.Title);
            result.HasErrors.Should().BeFalse();
        }
    }
}