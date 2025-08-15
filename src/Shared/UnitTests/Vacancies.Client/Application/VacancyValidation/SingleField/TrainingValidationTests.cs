using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.VacancyValidation.SingleField
{
    public class TrainingValidationTests : VacancyValidationTestsBase
    {
        [Fact]
        public void ErrorWhenProgrammeIsNull()
        {
            var vacancy = new Vacancy
            {
                ProgrammeId = null
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.TrainingProgramme);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.ProgrammeId)}");
            result.Errors[0].ErrorCode.Should().Be("25");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.TrainingProgramme);
        }

        [Fact]
        public void NoErrorsWhenClosingDateIsValid()
        {
            var vacancy = new Vacancy
            {
                ProgrammeId = "123",
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.TrainingProgramme);

            result.HasErrors.Should().BeFalse();
            result.Errors.Should().HaveCount(0);
        }

        [Fact]
        public void ErrorWhenDoesNotExist()
        {
            var vacancy = new Vacancy
            {
                ProgrammeId = "abc"
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.TrainingExpiryDate);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].ErrorCode.Should().Be("260");
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void IdMustHaveAValue(string idValue)
        {
            var vacancy = new Vacancy
            {
                ProgrammeId = idValue
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.TrainingProgramme);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.ProgrammeId)}");
            result.Errors[0].ErrorCode.Should().Be("25");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.TrainingProgramme);
        }

        [Fact]
        public void ErrorWhenProgrammeIdIsWhitespace()
        {
            var vacancy = new Vacancy
            {
                ProgrammeId = "   ",
                TrainingProvider = new TrainingProvider
                {
                    Ukprn = 10000002
                }
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.TrainingProgramme);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(vacancy.ProgrammeId) && e.ErrorCode == "25");
        }
    }
}