using System;
using Esfa.Recruit.Vacancies.Client.Application.Exceptions;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Services;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Application.Validation
{
    public class OrganisationValidationTests
    {
        private IEntityValidator<Vacancy, VacancyRuleSet> _validator;

        public OrganisationValidationTests()
        {
            var timeProvider = new CurrentTimeProvider();

            _validator = new EntityValidator<Vacancy, VacancyRuleSet>(new FluentVacancyValidator(timeProvider));
        }

        [Fact]
        public void ShouldNotThrowExceptionIfOrganisationFieldsAreValid()
        {
            var vacancy = new Vacancy
            {
                OrganisationId = "1234",
                Location = new Address
                {
                    AddressLine1 = "1 New Street",
                    Postcode = "AB1 3SD"
                }
            };

            Action act = () => _validator.ValidateAndThrow(vacancy, VacancyRuleSet.OrganisationId | VacancyRuleSet.OrganisationAddress);

            act.Should().NotThrow<EntityValidationException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void OrganisationIdMustBeSet(string organisationIdValue)
        {
            var vacancy = new Vacancy 
            {
                OrganisationId = organisationIdValue
            };

            Action act = () => _validator.ValidateAndThrow(vacancy, VacancyRuleSet.OrganisationId);

            var ex = act.Should().Throw<EntityValidationException>();
            ex.Which.ValidationResult.HasErrors.Should().BeTrue();
            ex.Which.ValidationResult.Errors.Count.Should().Be(1);
            ex.Which.ValidationResult.Errors[0].PropertyName.Should().Be(nameof(vacancy.OrganisationId));
            ex.Which.ValidationResult.Errors[0].ErrorCode.Should().Be("4");
            ex.Which.ValidationResult.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.OrganisationId);
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

            Action act = () => _validator.ValidateAndThrow(vacancy, VacancyRuleSet.OrganisationAddress);

            var ex = act.Should().Throw<EntityValidationException>();
            ex.Which.ValidationResult.HasErrors.Should().BeTrue();
            ex.Which.ValidationResult.Errors.Count.Should().Be(1);
            ex.Which.ValidationResult.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Location)}.{nameof(vacancy.Location.AddressLine1)}");
            ex.Which.ValidationResult.Errors[0].ErrorCode.Should().Be("5");
            ex.Which.ValidationResult.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.OrganisationAddress);
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

            Action act = () => _validator.ValidateAndThrow(vacancy, VacancyRuleSet.OrganisationAddress);

            var ex = act.Should().Throw<EntityValidationException>();
            ex.Which.ValidationResult.HasErrors.Should().BeTrue();
            ex.Which.ValidationResult.Errors.Count.Should().Be(1);
            ex.Which.ValidationResult.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Location)}.{nameof(vacancy.Location.AddressLine1)}");
            ex.Which.ValidationResult.Errors[0].ErrorCode.Should().Be("6");
            ex.Which.ValidationResult.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.OrganisationAddress);
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

            Action act = () => _validator.ValidateAndThrow(vacancy, VacancyRuleSet.OrganisationAddress);

            var ex = act.Should().Throw<EntityValidationException>();
            ex.Which.ValidationResult.HasErrors.Should().BeTrue();
            ex.Which.ValidationResult.Errors.Count.Should().Be(1);
            ex.Which.ValidationResult.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Location)}.{nameof(vacancy.Location.AddressLine1)}");
            ex.Which.ValidationResult.Errors[0].ErrorCode.Should().Be("7");
            ex.Which.ValidationResult.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.OrganisationAddress);
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

            Action act = () => _validator.ValidateAndThrow(vacancy, VacancyRuleSet.OrganisationAddress);

            var ex = act.Should().Throw<EntityValidationException>();
            ex.Which.ValidationResult.HasErrors.Should().BeTrue();
            ex.Which.ValidationResult.Errors.Count.Should().Be(1);
            ex.Which.ValidationResult.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Location)}.{nameof(vacancy.Location.AddressLine2)}");
            ex.Which.ValidationResult.Errors[0].ErrorCode.Should().Be("6");
            ex.Which.ValidationResult.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.OrganisationAddress);
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

            Action act = () => _validator.ValidateAndThrow(vacancy, VacancyRuleSet.OrganisationAddress);

            var ex = act.Should().Throw<EntityValidationException>();
            ex.Which.ValidationResult.HasErrors.Should().BeTrue();
            ex.Which.ValidationResult.Errors.Count.Should().Be(1);
            ex.Which.ValidationResult.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Location)}.{nameof(vacancy.Location.AddressLine2)}");
            ex.Which.ValidationResult.Errors[0].ErrorCode.Should().Be("7");
            ex.Which.ValidationResult.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.OrganisationAddress);
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

            Action act = () => _validator.ValidateAndThrow(vacancy, VacancyRuleSet.OrganisationAddress);

            var ex = act.Should().Throw<EntityValidationException>();
            ex.Which.ValidationResult.HasErrors.Should().BeTrue();
            ex.Which.ValidationResult.Errors.Count.Should().Be(1);
            ex.Which.ValidationResult.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Location)}.{nameof(vacancy.Location.AddressLine3)}");
            ex.Which.ValidationResult.Errors[0].ErrorCode.Should().Be("6");
            ex.Which.ValidationResult.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.OrganisationAddress);
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

            Action act = () => _validator.ValidateAndThrow(vacancy, VacancyRuleSet.OrganisationAddress);

            var ex = act.Should().Throw<EntityValidationException>();
            ex.Which.ValidationResult.HasErrors.Should().BeTrue();
            ex.Which.ValidationResult.Errors.Count.Should().Be(1);
            ex.Which.ValidationResult.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Location)}.{nameof(vacancy.Location.AddressLine3)}");
            ex.Which.ValidationResult.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.OrganisationAddress);
            ex.Which.ValidationResult.Errors[0].ErrorCode.Should().Be("7");
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

            Action act = () => _validator.ValidateAndThrow(vacancy, VacancyRuleSet.OrganisationAddress);

            var ex = act.Should().Throw<EntityValidationException>();
            ex.Which.ValidationResult.HasErrors.Should().BeTrue();
            ex.Which.ValidationResult.Errors.Count.Should().Be(1);
            ex.Which.ValidationResult.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Location)}.{nameof(vacancy.Location.AddressLine4)}");
            ex.Which.ValidationResult.Errors[0].ErrorCode.Should().Be("6");
            ex.Which.ValidationResult.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.OrganisationAddress);
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

            Action act = () => _validator.ValidateAndThrow(vacancy, VacancyRuleSet.OrganisationAddress);

            var ex = act.Should().Throw<EntityValidationException>();
            ex.Which.ValidationResult.HasErrors.Should().BeTrue();
            ex.Which.ValidationResult.Errors.Count.Should().Be(1);
            ex.Which.ValidationResult.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Location)}.{nameof(vacancy.Location.AddressLine4)}");
            ex.Which.ValidationResult.Errors[0].ErrorCode.Should().Be("7");
            ex.Which.ValidationResult.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.OrganisationAddress);
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

            Action act = () => _validator.ValidateAndThrow(vacancy, VacancyRuleSet.OrganisationAddress);

            var ex = act.Should().Throw<EntityValidationException>();
            ex.Which.ValidationResult.HasErrors.Should().BeTrue();
            ex.Which.ValidationResult.Errors.Count.Should().Be(1);
            ex.Which.ValidationResult.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Location)}.{nameof(vacancy.Location.Postcode)}");
            ex.Which.ValidationResult.Errors[0].ErrorCode.Should().Be("8");
            ex.Which.ValidationResult.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.OrganisationAddress);
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

            Action act = () => _validator.ValidateAndThrow(vacancy, VacancyRuleSet.OrganisationAddress);

            var ex = act.Should().Throw<EntityValidationException>();
            ex.Which.ValidationResult.HasErrors.Should().BeTrue();
            ex.Which.ValidationResult.Errors.Count.Should().Be(1);
            ex.Which.ValidationResult.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Location)}.{nameof(vacancy.Location.Postcode)}");
            ex.Which.ValidationResult.Errors[0].ErrorCode.Should().Be("9");
            ex.Which.ValidationResult.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.OrganisationAddress);
        }
    }
}