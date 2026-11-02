using Esfa.Recruit.UnitTests.Vacancies.Client.Application;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.VacancyValidation.SingleField;

public partial class EmployerContactDetailValidationTests : VacancyValidationTestsBase
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("joebloggs@company.com")]
    public void NoErrorsWhenEmployerContactEmailIsValid(string emailAddress)
    {
        var vacancy = new Vacancy
        {
            EmployerContact = new ContactDetail { Email = emailAddress} 
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerContactDetails);

        result.HasErrors.Should().BeFalse();
        result.Errors.Should().HaveCount(0);
    }

    [Fact]
    public void EmployerContactEmailMustBe100CharactersOrLess()
    {
        var vacancy = new Vacancy
        {
            EmployerContact = new ContactDetail { Email = "name@".PadRight(101, 'w') }
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerContactDetails);

        result.HasErrors.Should().BeTrue();
        result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.EmployerContact)}.{nameof(vacancy.EmployerContact.Email)}");
        result.Errors[0].ErrorCode.Should().Be("92");
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.EmployerContactDetails);
    }

    [Theory]
    [MemberData(nameof(TestData.BlacklistedCharacters), MemberType = typeof(TestData))]
    public void EmployerContactEmailMustNotContainInvalidCharacters(string invalidChar)
    {
        var vacancy = new Vacancy
        {
            EmployerContact = new ContactDetail { Email = invalidChar }
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerContactDetails);

        result.HasErrors.Should().BeTrue();
        result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.EmployerContact)}.{nameof(vacancy.EmployerContact.Email)}");
        result.Errors[0].ErrorCode.Should().Be("93");
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.EmployerContactDetails);
    }

    [Fact]
    public void EmployerContactEmailMustBeValidEmailFormat()
    {
        var vacancy = new Vacancy
        {
            EmployerContact = new ContactDetail { Email = "joe" }
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerContactDetails);

        result.HasErrors.Should().BeTrue();
        result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.EmployerContact)}.{nameof(vacancy.EmployerContact.Email)}");
        result.Errors[0].ErrorCode.Should().Be("94");
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.EmployerContactDetails);
    }
}