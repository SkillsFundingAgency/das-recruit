using Esfa.Recruit.UnitTests.Vacancies.Client.Application;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.VacancyValidation.SingleField;

public partial class EmployerContactDetailValidationTests : VacancyValidationTestsBase
{
    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase("joebloggs@company.com")]
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

    [Test]
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

    [Test]
    [TestCaseSource(typeof(TestData), nameof(TestData.BlacklistedCharacters))]
    public void EmployerContactEmailMustNotContainInvalidCharacters(char invalidChar)
    {
        var vacancy = new Vacancy
        {
            EmployerContact = new ContactDetail { Email = invalidChar.ToString() }
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerContactDetails);

        result.HasErrors.Should().BeTrue();
        result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.EmployerContact)}.{nameof(vacancy.EmployerContact.Email)}");
        result.Errors[0].ErrorCode.Should().Be("93");
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.EmployerContactDetails);
    }

    [Test]
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
