using System;
using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Application.Exceptions;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Services;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Application.Validation
{
    public class ClosingDateTests
    {
        private IEntityValidator<Vacancy, VacancyRuleSet> _validator;

        public ClosingDateTests()
        {
            var timeProvider = new CurrentTimeProvider();

            _validator = new EntityValidator<Vacancy, VacancyRuleSet>(new FluentVacancyValidator(timeProvider));
        }

        public static IEnumerable<object[]> InvalidClosingDates =>
            new List<object[]>
            {
                new object[] { DateTime.UtcNow.Date },
                new object[] { DateTime.UtcNow },
                new object[] { DateTime.UtcNow.AddDays(-1) }
            };

        [Fact]
        public void ClosingDateMustHaveAValue()
        {
            var vacancy = new Vacancy 
            {
                ClosingDate = null
            };
            
            Action act = () => _validator.ValidateAndThrow(vacancy, VacancyRuleSet.ClosingDate);

            var ex = act.Should().Throw<EntityValidationException>();
            ex.Which.ValidationResult.HasErrors.Should().BeTrue();
            ex.Which.ValidationResult.Errors.Count.Should().Be(1);
            ex.Which.ValidationResult.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.ClosingDate)}");
            ex.Which.ValidationResult.Errors[0].ErrorCode.Should().Be("16");
            ex.Which.ValidationResult.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.ClosingDate);
        }

        [Theory]
        [MemberData(nameof(InvalidClosingDates))]
        public void ClosingDateMustBeGreaterThanToday(DateTime closingDateValue)
        {
            var vacancy = new Vacancy
            {
                ClosingDate = closingDateValue
            };

            Action act = () => _validator.ValidateAndThrow(vacancy, VacancyRuleSet.ClosingDate);

            var ex = act.Should().Throw<EntityValidationException>();
            ex.Which.ValidationResult.HasErrors.Should().BeTrue();
            ex.Which.ValidationResult.Errors.Count.Should().Be(1);
            ex.Which.ValidationResult.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.ClosingDate)}");
            ex.Which.ValidationResult.Errors[0].ErrorCode.Should().Be("18");
            ex.Which.ValidationResult.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.ClosingDate);
        }
    }
}