using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Application.VacancyValidation.SingleField
{
    public class TrainingProviderUkprnValidationTests : VacancyValidationTestsBase
    {
        [Fact]
        public void NoErrorsWhenTrainingProviderUkprnIsValid()
        {
            var vacancy = new Vacancy
            {
                TrainingProvider = new TrainingProvider { Ukprn = 12345678 }
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.TrainingProvider);

            result.HasErrors.Should().BeFalse();
            result.Errors.Should().HaveCount(0);
        }

        [Fact]
        public void EmptyUkprnNotAllowed()
        {
            var vacancy = new Vacancy
            {
                TrainingProvider = new TrainingProvider { Ukprn = null }
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.TrainingProvider);

            result.HasErrors.Should().BeTrue();
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.TrainingProvider));
            result.Errors[0].ErrorCode.Should().Be("101");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.TrainingProvider);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1234)]
        [InlineData(123456789)]
        public void TrainingProviderUkprnMustBe8Digits(long? ukprn)
        {
            var vacancy = new Vacancy
            {
                TrainingProvider = new TrainingProvider { Ukprn = ukprn }
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.TrainingProvider);

            result.HasErrors.Should().BeTrue();
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.TrainingProvider));
            result.Errors[0].ErrorCode.Should().Be("99");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.TrainingProvider);
        }
    }
}