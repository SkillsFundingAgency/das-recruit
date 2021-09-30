using System;
using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
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
                new TestApprenticeshipProgramme {Id = "123", EffectiveTo = null}
            };

            MockApprenticeshipProgrammeProvider.Setup(x => x.GetApprenticeshipProgrammesAsync(false)).ReturnsAsync(programmes);

            var result = Validator.Validate(vacancy, VacancyRuleSet.TrainingExpiryDate);

            result.HasErrors.Should().BeFalse();
            result.Errors.Should().HaveCount(0);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public void TrainingIsValidIfExpiryDateOfTrainingIsAfterStartDate(int daysAfterStartDate)
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

            var vacancy = new Vacancy
            {
                StartDate = startDate,
                ProgrammeId = "123"
            };

            var programmes = new List<IApprenticeshipProgramme>
            {
                new TestApprenticeshipProgramme {Id = "123", EffectiveTo = startDate.AddDays(-1)}
            };

            MockApprenticeshipProgrammeProvider.Setup(x => x.GetApprenticeshipProgrammesAsync(false)).ReturnsAsync(programmes);

            var result = Validator.Validate(vacancy, VacancyRuleSet.TrainingExpiryDate);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be(nameof(Vacancy.StartDate));
            result.Errors[0].ErrorCode.Should().Be(ErrorCodes.TrainingExpiryDate);
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.TrainingExpiryDate);
        }
    }
}