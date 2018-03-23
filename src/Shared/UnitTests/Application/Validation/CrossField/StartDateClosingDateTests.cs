using System;
using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Services;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Application.Validation.SingleField
{
    public class StartDateClosingDateTests
    {
        private IEntityValidator<Vacancy, VacancyRuleSet> _validator;

        public StartDateClosingDateTests()
        {
            var timeProvider = new CurrentTimeProvider();

            _validator = new EntityValidator<Vacancy, VacancyRuleSet>(new FluentVacancyValidator(timeProvider));
        }

        [Fact]
        public void NoErrorsWhenFieldsAreValid()
        {
            var vacancy = new Vacancy
            {
                StartDate = DateTime.UtcNow.AddDays(5),
                ClosingDate = DateTime.UtcNow.AddDays(4)
            };

            var result = _validator.Validate(vacancy, VacancyRuleSet.StartDateEndDate);

            result.HasErrors.Should().BeFalse();
            result.Errors.Should().HaveCount(0);
        }

        [Fact]
        public void StartDateCantBeTheSameAsClosingDate()
        {
            var vacancy = new Vacancy
            {
                StartDate = DateTime.UtcNow,
                ClosingDate = DateTime.UtcNow
            };
            
            var result = _validator.Validate(vacancy, VacancyRuleSet.StartDateEndDate);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be(string.Empty);
            result.Errors[0].ErrorCode.Should().Be("24");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.StartDateEndDate);
        }

        [Fact]
        public void StartDateCantBeBeforeClosingDate()
        {
            var vacancy = new Vacancy
            {
                StartDate = DateTime.UtcNow,
                ClosingDate = DateTime.UtcNow.AddDays(1)
            };

            var result = _validator.Validate(vacancy, VacancyRuleSet.StartDateEndDate);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be(string.Empty);
            result.Errors[0].ErrorCode.Should().Be("24");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.StartDateEndDate);
        }
    }
}