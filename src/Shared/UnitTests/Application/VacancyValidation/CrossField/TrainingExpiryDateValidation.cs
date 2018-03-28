using System;
using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Projections;
using FluentAssertions;
using Moq;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Application.VacancyValidation.CrossField
{
    public class TrainingExpiryDateValidation : VacancyValidationTestsBase
    {
        [Fact]
        public void TrainingIsValidIfExpiryDateOfTrainingIsNull()
        {
            var vacancy = new Vacancy
            {
                Programme = new Programme
                {
                    Id = "123"
                }
            };

            ApprenticeshipProgrammes programmes = new ApprenticeshipProgrammes();

            programmes.Programmes = new List<ApprenticeshipProgramme>
            {
                new ApprenticeshipProgramme { Id = "123", EffectiveTo = null }
            };

            MockQueryStoreReader.Setup(x => x.GetApprenticeshipProgrammesAsync()).ReturnsAsync(programmes);

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
                Programme = new Programme
                {
                    Id = "123"
                }
            };

            ApprenticeshipProgrammes programmes = new ApprenticeshipProgrammes();

            programmes.Programmes = new List<ApprenticeshipProgramme>
            {
                new ApprenticeshipProgramme { Id = "123", EffectiveTo = startDate.AddDays(daysAfterStartDate) }
            };

            MockQueryStoreReader.Setup(x => x.GetApprenticeshipProgrammesAsync()).ReturnsAsync(programmes);

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
                Programme = new Programme
                {
                    Id = "123"
                }
            };

            ApprenticeshipProgrammes programmes = new ApprenticeshipProgrammes();

            programmes.Programmes = new List<ApprenticeshipProgramme>
            {
                new ApprenticeshipProgramme { Id = "123", EffectiveTo = startDate.AddDays(-1) }
            };

            MockQueryStoreReader.Setup(x => x.GetApprenticeshipProgrammesAsync()).ReturnsAsync(programmes);

            var result = Validator.Validate(vacancy, VacancyRuleSet.TrainingExpiryDate);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be(string.Empty);
            result.Errors[0].ErrorCode.Should().Be("26");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.TrainingExpiryDate);
        }
    }
}