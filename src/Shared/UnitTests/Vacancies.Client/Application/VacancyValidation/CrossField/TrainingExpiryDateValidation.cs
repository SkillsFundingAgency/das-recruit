using System;
using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using FluentAssertions;
using Moq;
using Xunit;

namespace Esfa.Recruit.UnitTests.Vacancies.Client.Application.VacancyValidation.CrossField
{
    public class TrainingExpiryDateValidation : VacancyValidationTestsBase
    {
        [Fact]
        public void TrainingIsValidIfExpiryDateOfTrainingIsNull()
        {
            var vacancy = new Vacancy
            {
                ProgrammeId = "123",
                StartDate = DateTime.UtcNow
            };

            var programmes = new List<IApprenticeshipProgramme>
            {
                new TestApprenticeshipProgramme {Id = "123", LastDateStarts = null, EffectiveTo = null}
            };

            MockApprenticeshipProgrammeProvider.Setup(x => x.GetApprenticeshipProgrammesAsync(false)).ReturnsAsync(programmes);

            var result = Validator.Validate(vacancy, VacancyRuleSet.TrainingExpiryDate);

            result.HasErrors.Should().BeFalse();
            result.Errors.Should().HaveCount(0);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public void TrainingIsValidIfLastDateStartsOfTrainingIsAfterStartDate(int daysAfterStartDate)
        {
            var startDate = DateTime.UtcNow.Date.AddDays(20);

            var vacancy = new Vacancy
            {
                StartDate = startDate,
                ProgrammeId = "123"
            };

            var programmes = new List<IApprenticeshipProgramme>
            {
                new TestApprenticeshipProgramme {Id = "123", LastDateStarts = startDate.AddDays(daysAfterStartDate)}
            };

            MockApprenticeshipProgrammeProvider.Setup(x => x.GetApprenticeshipProgrammesAsync(false)).ReturnsAsync(programmes);

            var result = Validator.Validate(vacancy, VacancyRuleSet.TrainingExpiryDate);

            result.HasErrors.Should().BeFalse();
            result.Errors.Should().HaveCount(0);
        }
        
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public void TrainingIsValidIfEffectiveToDateOfTrainingIsAfterStartDate(int daysAfterStartDate)
        {
            var startDate = DateTime.UtcNow.Date.AddDays(20);

            var vacancy = new Vacancy
            {
                StartDate = startDate,
                ProgrammeId = "123"
            };

            var programmes = new List<IApprenticeshipProgramme>
            {
                new TestApprenticeshipProgramme {Id = "123", EffectiveTo = startDate.AddDays(daysAfterStartDate)}
            };

            MockApprenticeshipProgrammeProvider.Setup(x => x.GetApprenticeshipProgrammesAsync(false)).ReturnsAsync(programmes);

            var result = Validator.Validate(vacancy, VacancyRuleSet.TrainingExpiryDate);

            result.HasErrors.Should().BeFalse();
            result.Errors.Should().HaveCount(0);
        }

        [Fact]
        public void TrainingIsNotValidIfExpiryDateOfTrainingBeforeStartDate()
        {
            var startDate = DateTime.UtcNow.Date.AddDays(20);
            var effectiveTo = startDate.AddDays(-1);

            var vacancy = new Vacancy
            {
                StartDate = startDate,
                ProgrammeId = "123"
            };

            var programmes = new List<IApprenticeshipProgramme>
            {
                new TestApprenticeshipProgramme {Id = "123", EffectiveTo = effectiveTo}
            };

            MockApprenticeshipProgrammeProvider.Setup(x => x.GetApprenticeshipProgrammesAsync(false)).ReturnsAsync(programmes);

            var result = Validator.Validate(vacancy, VacancyRuleSet.TrainingExpiryDate);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be(nameof(Vacancy.StartDate));
            result.Errors[0].ErrorCode.Should().Be(ErrorCodes.TrainingExpiryDate);
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.TrainingExpiryDate);
            result.Errors[0].ErrorMessage.Should().Be($"Start date must be on or before {effectiveTo.AsGdsDate()} as this is the last day for new starters for the training course you have selected. If you don’t want to change the start date, you can change the training course.");
        }
        
        [Fact]
        public void TrainingIsNotValidIfLastDateForNewStartsOfTrainingBeforeStartDate()
        {
            var startDate = DateTime.UtcNow.Date.AddDays(20);
            var lastDateStarts = startDate.AddDays(-1);

            var vacancy = new Vacancy
            {
                StartDate = startDate,
                ProgrammeId = "123"
            };

            var programmes = new List<IApprenticeshipProgramme>
            {
                new TestApprenticeshipProgramme {Id = "123", LastDateStarts = lastDateStarts}
            };

            MockApprenticeshipProgrammeProvider.Setup(x => x.GetApprenticeshipProgrammesAsync(false)).ReturnsAsync(programmes);

            var result = Validator.Validate(vacancy, VacancyRuleSet.TrainingExpiryDate);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be(nameof(Vacancy.StartDate));
            result.Errors[0].ErrorCode.Should().Be(ErrorCodes.TrainingExpiryDate);
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.TrainingExpiryDate);
            result.Errors[0].ErrorMessage.Should().Be($"Start date must be on or before {lastDateStarts.AsGdsDate()} as this is the last day for new starters for the training course you have selected. If you don’t want to change the start date, you can change the training course.");
        }
    }
}