using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.VacancyValidation.SingleField;

public class EmployerValidationTests : VacancyValidationTestsBase
{
    [Fact]
    public void NoErrorsWhenEmployerFieldsAreValid()
    {
        var vacancy = new Vacancy
        {
            EmployerName = "Test Org",
            EmployerLocation = new Address
            {
                AddressLine1 = "1 New Street",
                Postcode = "AB1 3SD"
            }
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerName | VacancyRuleSet.EmployerAddress);

        result.HasErrors.Should().BeFalse();
        result.Errors.Should().HaveCount(0);
    }
        
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void EmployerMustBeSet(string organisationValue)
    {
        var vacancy = new Vacancy 
        {
            EmployerName = organisationValue,
            SourceOrigin = SourceOrigin.ProviderWeb
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerName);

        result.HasErrors.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].PropertyName.Should().Be(nameof(vacancy.EmployerName));
        result.Errors[0].ErrorCode.Should().Be("4");
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.EmployerName);
    }

    [Fact]
    public void ShouldErrorIfEmployerLocationIsNull()
    {
        var vacancy = new Vacancy
        {
            EmployerLocation = null
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerAddress);

        result.HasErrors.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].PropertyName.Should().Be(nameof(vacancy.EmployerLocation));
        result.Errors[0].ErrorCode.Should().Be("98");
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.EmployerAddress);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void EmployerAddressLine1MustBeSet(string addressValue)
    {
        var vacancy = new Vacancy
        {
            EmployerLocation = new Address
            {
                AddressLine1 = addressValue,
                Postcode = "AB12 3DZ"   
            }
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerAddress);

        result.HasErrors.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.EmployerLocation)}.{nameof(vacancy.EmployerLocation.AddressLine1)}");
        result.Errors[0].ErrorCode.Should().Be("5");
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.EmployerAddress);
    }

    [Theory]
    [InlineData("<")]
    [InlineData(">")]
    public void EmployerAddressLine1MustContainValidCharacters(string testValue)
    {
        var vacancy = new Vacancy
        {
            EmployerLocation = new Address
            {
                AddressLine1 = testValue,
                Postcode = "AB12 3DZ"
            }
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerAddress);

        result.HasErrors.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.EmployerLocation)}.{nameof(vacancy.EmployerLocation.AddressLine1)}");
        result.Errors[0].ErrorCode.Should().Be("6");
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.EmployerAddress);
    }

    [Fact]
    public void EmployerAddressLine1CannotBeLongerThan100Characters()
    {
        var vacancy = new Vacancy
        {
            EmployerLocation = new Address
            {
                AddressLine1 = new string('a', 101),
                Postcode = "AB12 3DZ"
            }
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerAddress);

        result.HasErrors.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.EmployerLocation)}.{nameof(vacancy.EmployerLocation.AddressLine1)}");
        result.Errors[0].ErrorCode.Should().Be("7");
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.EmployerAddress);
    }

    [Theory]
    [InlineData("<")]
    [InlineData(">")]
    public void EmployerAddressLine2MustContainValidCharacters(string testValue)
    {
        var vacancy = new Vacancy
        {
            EmployerLocation = new Address
            {
                AddressLine1 = "2 New Street",
                AddressLine2 = testValue,
                Postcode = "AB12 3DZ"
            }
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerAddress);

        result.HasErrors.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.EmployerLocation)}.{nameof(vacancy.EmployerLocation.AddressLine2)}");
        result.Errors[0].ErrorCode.Should().Be("6");
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.EmployerAddress);
    }

    [Fact]
    public void EmployerAddressLine2CannotBeLongerThan100Characters()
    {
        var vacancy = new Vacancy
        {
            EmployerLocation = new Address
            {
                AddressLine1 = "2 New Street",
                AddressLine2 = new String('a', 101),
                Postcode = "AB12 3DZ"
            }
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerAddress);

        result.HasErrors.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.EmployerLocation)}.{nameof(vacancy.EmployerLocation.AddressLine2)}");
        result.Errors[0].ErrorCode.Should().Be("7");
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.EmployerAddress);
    }

    [Theory]
    [InlineData("<")]
    [InlineData(">")]
    public void EmployerAddressLine3MustContainValidCharacters(string testValue)
    {
        var vacancy = new Vacancy
        {
            EmployerLocation = new Address
            {
                AddressLine1 = "2 New Street",
                AddressLine3 = testValue,
                Postcode = "AB12 3DZ"
            }
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerAddress);

        result.HasErrors.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.EmployerLocation)}.{nameof(vacancy.EmployerLocation.AddressLine3)}");
        result.Errors[0].ErrorCode.Should().Be("6");
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.EmployerAddress);
    }

    [Fact]
    public void EmployerAddressLine3CannotBeLongerThan100Characters()
    {
        var vacancy = new Vacancy
        {
            EmployerLocation = new Address
            {
                AddressLine1 = "2 New Street",
                AddressLine3 = new string('a', 101),
                Postcode = "AB12 3DZ"
            }
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerAddress);

        result.HasErrors.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.EmployerLocation)}.{nameof(vacancy.EmployerLocation.AddressLine3)}");
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.EmployerAddress);
        result.Errors[0].ErrorCode.Should().Be("7");
    }

    [Theory]
    [InlineData("<")]
    [InlineData(">")]
    public void EmployerAddressLine4MustContainValidCharacters(string testValue)
    {
        var vacancy = new Vacancy
        {
            EmployerLocation = new Address
            {
                AddressLine1 = "2 New Street",
                AddressLine4 = testValue,
                Postcode = "AB12 3DZ"
            }
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerAddress);

        result.HasErrors.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.EmployerLocation)}.{nameof(vacancy.EmployerLocation.AddressLine4)}");
        result.Errors[0].ErrorCode.Should().Be("6");
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.EmployerAddress);
    }

    [Fact]
    public void EmployerAddressLine4CannotBeLongerThan100Characters()
    {
        var vacancy = new Vacancy
        {
            EmployerLocation = new Address
            {
                AddressLine1 = "2 New Street",
                AddressLine4 = new string('a', 101),
                Postcode = "AB12 3DZ"
            }
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerAddress);

        result.HasErrors.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.EmployerLocation)}.{nameof(vacancy.EmployerLocation.AddressLine4)}");
        result.Errors[0].ErrorCode.Should().Be("7");
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.EmployerAddress);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void EmployerPostCodeMustBeSet(string postCodeValue)
    {
        var vacancy = new Vacancy
        {
            EmployerLocation = new Address
            {
                AddressLine1 = "2 New Street",
                Postcode = postCodeValue
            }
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerAddress);

        result.HasErrors.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.EmployerLocation)}.{nameof(vacancy.EmployerLocation.Postcode)}");
        result.Errors[0].ErrorCode.Should().Be("8");
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.EmployerAddress);
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("AAAAAA")]
    [InlineData("AS123 1JJ")]
    public void EmployerPostCodeMustBeValidFormat(string postCodeValue)
    {
        var vacancy = new Vacancy
        {
            EmployerLocation = new Address
            {
                AddressLine1 = "2 New Street",
                Postcode = postCodeValue
            }
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerAddress);

        result.HasErrors.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.EmployerLocation)}.{nameof(vacancy.EmployerLocation.Postcode)}");
        result.Errors[0].ErrorCode.Should().Be("9");
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.EmployerAddress);
    }
}