using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.UnitTests.Vacancies.Client.Application.VacancyValidation.SingleField
{
    public class AnonymousReasonTests : VacancyValidationTestsBase
    {
        [Fact]
        public void EmployerWeb_Anonymous_ShouldValidateEmpty()        
        {
            var vacancy = new Vacancy()
            {
                EmployerName = "a valid anonymous name",
                EmployerNameOption = EmployerNameOption.Anonymous,
                SourceOrigin = SourceOrigin.EmployerWeb,
                AnonymousReason = null
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerNameOption);

            result.HasErrors.Should().BeTrue();
            result.Errors.Count.Should().Be(1);
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.AnonymousReason));
            result.Errors[0].ErrorCode.Should().Be("408");
        }

        [Fact]
        public void EmployerWeb_Anonymous_ShouldValidateSpecialCharactersAndLength()
        {
            var vacancy = new Vacancy() {
                EmployerName = "a valid anonymous name",
                EmployerNameOption = EmployerNameOption.Anonymous,
                SourceOrigin = SourceOrigin.EmployerWeb,
                AnonymousReason = "�$$%$%�$<>" + new string('a', 200),
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerNameOption);

            result.HasErrors.Should().BeTrue();
            result.Errors.Count.Should().Be(2);
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.AnonymousReason));
            result.Errors[0].ErrorCode.Should().Be("409");
            result.Errors[1].PropertyName.Should().Be(nameof(vacancy.AnonymousReason));
            result.Errors[1].ErrorCode.Should().Be("410");
        }

        [Fact]
        public void EmployerWeb_Anonymous_ShouldFailIfContainsWordsFromTheProfanityList()
        {
            var vacancy = new Vacancy()
            {
                EmployerName = "a valid anonymous name",
                EmployerNameOption = EmployerNameOption.Anonymous,
                SourceOrigin = SourceOrigin.EmployerWeb,
                AnonymousReason = "test anonymous dangleberry"
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerNameOption);
            result.HasErrors.Should().BeTrue();
            result.Errors.Count.Should().Be(1);
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.AnonymousReason));
            result.Errors[0].ErrorCode.Should().Be("5");
        }
    }
}