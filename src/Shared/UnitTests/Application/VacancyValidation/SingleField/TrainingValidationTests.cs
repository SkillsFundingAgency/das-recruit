using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using UnitTests.Application.VacancyValidation;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Application.VacancyValidation.SingleField
{
    public class TrainingValidationTests : VacancyValidationTestsBase
    {
        [Fact]
        public void NoErrorsWhenClosingDateIsValid()
        {
            var vacancy = new Vacancy
            {
                Programme = new Programme
                {
                    Id = "11",
                    Level = 2,
                    LevelName = "Level",
                    Title = "Test Standard",
                    TrainingType = TrainingType.Standard
                }
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.TrainingProgramme);

            result.HasErrors.Should().BeFalse();
            result.Errors.Should().HaveCount(0);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void IdMustHaveAValue(string idValue)
        {
            var vacancy = new Vacancy 
            {
                Programme = new Programme
                {
                    Id = idValue,
                    Level = 2,
                    LevelName = "Level",
                    Title = "Test Standard",
                    TrainingType = TrainingType.Standard
                }
            };
            
            var result = Validator.Validate(vacancy, VacancyRuleSet.TrainingProgramme);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Programme)}.{nameof(vacancy.Programme.Id)}");
            result.Errors[0].ErrorCode.Should().Be("25");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.TrainingProgramme);
        }
    }
}