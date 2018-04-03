using System;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Application.VacancyValidation.SingleField
{
    public class OrganisationValidationTests : VacancyValidationTestsBase
    {
        [Fact]
        public void NoErrorsWhenOrganisationFieldsAreValid()
        {
            var vacancy = new Vacancy
            {
                OrganisationName = "Test Org",
                Location = new Address
                {
                    AddressLine1 = "1 New Street",
                    Postcode = "AB1 3SD"
                }
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.OrganisationName | VacancyRuleSet.OrganisationAddress);

            result.HasErrors.Should().BeFalse();
            result.Errors.Should().HaveCount(0);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void OrganisationMustBeSet(string organisationValue)
        {
            var vacancy = new Vacancy 
            {
                OrganisationName = organisationValue
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.OrganisationName);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.OrganisationName));
            result.Errors[0].ErrorCode.Should().Be("4");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.OrganisationName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void OrganisationAddressLine1MustBeSet(string addressValue)
        {
            var vacancy = new Vacancy
            {
                Location = new Address
                {
                    AddressLine1 = addressValue,
                    Postcode = "AB12 3DZ"   
                }
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.OrganisationAddress);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Location)}.{nameof(vacancy.Location.AddressLine1)}");
            result.Errors[0].ErrorCode.Should().Be("5");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.OrganisationAddress);
        }

        [Theory]
        [InlineData("<")]
        [InlineData(">")]
        public void OrganisationAddressLine1MustContainValidCharacters(string testValue)
        {
            var vacancy = new Vacancy
            {
                Location = new Address
                {
                    AddressLine1 = testValue,
                    Postcode = "AB12 3DZ"
                }
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.OrganisationAddress);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Location)}.{nameof(vacancy.Location.AddressLine1)}");
            result.Errors[0].ErrorCode.Should().Be("6");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.OrganisationAddress);
        }

        [Fact]
        public void OrganisationAddressLine1CannotBeLongerThan100Characters()
        {
            var vacancy = new Vacancy
            {
                Location = new Address
                {
                    AddressLine1 = new string('a', 101),
                    Postcode = "AB12 3DZ"
                }
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.OrganisationAddress);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Location)}.{nameof(vacancy.Location.AddressLine1)}");
            result.Errors[0].ErrorCode.Should().Be("7");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.OrganisationAddress);
        }

        [Theory]
        [InlineData("<")]
        [InlineData(">")]
        public void OrganisationAddressLine2MustContainValidCharacters(string testValue)
        {
            var vacancy = new Vacancy
            {
                Location = new Address
                {
                    AddressLine1 = "2 New Street",
                    AddressLine2 = testValue,
                    Postcode = "AB12 3DZ"
                }
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.OrganisationAddress);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Location)}.{nameof(vacancy.Location.AddressLine2)}");
            result.Errors[0].ErrorCode.Should().Be("6");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.OrganisationAddress);
        }

        [Fact]
        public void OrganisationAddressLine2CannotBeLongerThan100Characters()
        {
            var vacancy = new Vacancy
            {
                Location = new Address
                {
                    AddressLine1 = "2 New Street",
                    AddressLine2 = new String('a', 101),
                    Postcode = "AB12 3DZ"
                }
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.OrganisationAddress);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Location)}.{nameof(vacancy.Location.AddressLine2)}");
            result.Errors[0].ErrorCode.Should().Be("7");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.OrganisationAddress);
        }

        [Theory]
        [InlineData("<")]
        [InlineData(">")]
        public void OrganisationAddressLine3MustContainValidCharacters(string testValue)
        {
            var vacancy = new Vacancy
            {
                Location = new Address
                {
                    AddressLine1 = "2 New Street",
                    AddressLine3 = testValue,
                    Postcode = "AB12 3DZ"
                }
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.OrganisationAddress);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Location)}.{nameof(vacancy.Location.AddressLine3)}");
            result.Errors[0].ErrorCode.Should().Be("6");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.OrganisationAddress);
        }

        [Fact]
        public void OrganisationAddressLine3CannotBeLongerThan100Characters()
        {
            var vacancy = new Vacancy
            {
                Location = new Address
                {
                    AddressLine1 = "2 New Street",
                    AddressLine3 = new string('a', 101),
                    Postcode = "AB12 3DZ"
                }
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.OrganisationAddress);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Location)}.{nameof(vacancy.Location.AddressLine3)}");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.OrganisationAddress);
            result.Errors[0].ErrorCode.Should().Be("7");
        }

        [Theory]
        [InlineData("<")]
        [InlineData(">")]
        public void OrganisationAddressLine4MustContainValidCharacters(string testValue)
        {
            var vacancy = new Vacancy
            {
                Location = new Address
                {
                    AddressLine1 = "2 New Street",
                    AddressLine4 = testValue,
                    Postcode = "AB12 3DZ"
                }
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.OrganisationAddress);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Location)}.{nameof(vacancy.Location.AddressLine4)}");
            result.Errors[0].ErrorCode.Should().Be("6");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.OrganisationAddress);
        }

        [Fact]
        public void OrganisationAddressLine4CannotBeLongerThan100Characters()
        {
            var vacancy = new Vacancy
            {
                Location = new Address
                {
                    AddressLine1 = "2 New Street",
                    AddressLine4 = new string('a', 101),
                    Postcode = "AB12 3DZ"
                }
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.OrganisationAddress);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Location)}.{nameof(vacancy.Location.AddressLine4)}");
            result.Errors[0].ErrorCode.Should().Be("7");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.OrganisationAddress);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void OrganisationPostCodeMustBeSet(string postCodeValue)
        {
            var vacancy = new Vacancy
            {
                Location = new Address
                {
                    AddressLine1 = "2 New Street",
                    Postcode = postCodeValue
                }
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.OrganisationAddress);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Location)}.{nameof(vacancy.Location.Postcode)}");
            result.Errors[0].ErrorCode.Should().Be("8");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.OrganisationAddress);
        }

        [Theory]
        [InlineData("12345")]
        [InlineData("AAAAAA")]
        [InlineData("AS123 1JJ")]
        public void OrganisationPostCodeMustBeValidFormat(string postCodeValue)
        {
            var vacancy = new Vacancy
            {
                Location = new Address
                {
                    AddressLine1 = "2 New Street",
                    Postcode = postCodeValue
                }
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.OrganisationAddress);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Location)}.{nameof(vacancy.Location.Postcode)}");
            result.Errors[0].ErrorCode.Should().Be("9");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.OrganisationAddress);
        }
    }
}