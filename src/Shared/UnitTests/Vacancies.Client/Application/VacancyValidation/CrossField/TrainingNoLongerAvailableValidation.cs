using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.UnitTests.Vacancies.Client.Application.VacancyValidation.CrossField;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.VacancyValidation.CrossField
{
    public class TrainingNoLongerAvailableValidation : VacancyValidationTestsBase
    {
        [Fact]
        public void Training_Is_Valid_If_Last_Date_For_Starts_Is_In_The_Future()
        {
            var mockTimeProvider = new Mock<ITimeProvider>();
            var vacancy = new Vacancy
            {
                ProgrammeId = "123",
                StartDate = DateTime.UtcNow.AddDays(2)
            };
            mockTimeProvider.Setup(x => x.Now).Returns(DateTime.UtcNow.AddDays(-10));
            TimeProvider = mockTimeProvider.Object;
            var programme = new TestApprenticeshipProgramme {Id = "123", LastDateStarts = DateTime.UtcNow.AddDays(12) };
            MockApprenticeshipProgrammeProvider.Setup(x => x.GetApprenticeshipProgrammeAsync(vacancy.ProgrammeId, null)).ReturnsAsync(programme);
            
            var result = Validator.Validate(vacancy, VacancyRuleSet.TrainingExpiryDate);

            result.HasErrors.Should().BeFalse();
            result.Errors.Should().HaveCount(0);
        }
        
        [Fact]
        public void Training_Is_Not_Valid_If_Last_Date_For_Starts_Is_In_The_Past()
        {
            var mockTimeProvider = new Mock<ITimeProvider>();
            mockTimeProvider.Setup(x => x.Now).Returns(DateTime.UtcNow.AddDays(8));
            TimeProvider = mockTimeProvider.Object;
            var programme = new TestApprenticeshipProgramme {Id = "123", LastDateStarts = DateTime.UtcNow.AddDays(7) };
            
            var vacancy = new Vacancy
            {
                ProgrammeId = "123",
                StartDate = DateTime.UtcNow.AddDays(10),
                TrainingProvider = new TrainingProvider
                {
                    Ukprn = 10000000
                }
            };
            MockApprenticeshipProgrammeProvider.Setup(x => x.GetApprenticeshipProgrammeAsync(vacancy.ProgrammeId, null)).ReturnsAsync(programme);
            var dateToDisplay = programme.LastDateStarts.HasValue
                ? programme.LastDateStarts.Value.AsGdsDate()
                : programme.EffectiveTo.Value.AsGdsDate();

            var result = Validator.Validate(vacancy, VacancyRuleSet.TrainingExpiryDate);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors.First()
                .ErrorMessage.Should()
                .Be(
                    $"Start date must be on or before {dateToDisplay} as this is the last day for new starters for the training course you have selected. If you don't want to change the start date, you can change the training course");
        }
        
        [Fact]
        public void Training_Is_Not_Valid_If_Effective_To_Date_Is_In_The_Past()
        {
            var mockTimeProvider = new Mock<ITimeProvider>();
            mockTimeProvider.Setup(x => x.Now).Returns(DateTime.UtcNow.AddDays(8));
            TimeProvider = mockTimeProvider.Object;
            var vacancy = new Vacancy
            {
                ProgrammeId = "123",
                StartDate = DateTime.UtcNow.AddDays(10),
                TrainingProvider = new TrainingProvider
                {
                    Ukprn = 10000000
                }
            };
            var programme = new TestApprenticeshipProgramme {Id = "123", EffectiveTo = DateTime.UtcNow.AddDays(7) };
            MockApprenticeshipProgrammeProvider.Setup(x => x.GetApprenticeshipProgrammeAsync(vacancy.ProgrammeId, null)).ReturnsAsync(programme);
            
            var dateToDisplay = programme.LastDateStarts.HasValue
                ? programme.LastDateStarts.Value.AsGdsDate()
                : programme.EffectiveTo.Value.AsGdsDate();

            var result = Validator.Validate(vacancy, VacancyRuleSet.TrainingExpiryDate);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors.First()
                .ErrorMessage.Should()
                .Be(
                    $"Start date must be on or before {dateToDisplay} as this is the last day for new starters for the training course you have selected. If you don't want to change the start date, you can change the training course");
        }

        [Fact]
        public void Training_Is_Not_Valid_If_No_Longer_Available()
        {
            var mockTimeProvider = new Mock<ITimeProvider>();
            mockTimeProvider.Setup(x => x.Now).Returns(DateTime.UtcNow.AddDays(8));
            TimeProvider = mockTimeProvider.Object;
            var programme = new TestApprenticeshipProgramme {Id = "123", EffectiveTo = DateTime.UtcNow.AddDays(7) };
            
            var vacancy = new Vacancy
            {
                ProgrammeId = "123",
                StartDate = DateTime.UtcNow.AddDays(10),
                TrainingProvider = new TrainingProvider
                {
                    Ukprn = 10000000
                }
            };
            MockApprenticeshipProgrammeProvider.Setup(x => x.GetApprenticeshipProgrammeAsync(vacancy.ProgrammeId, null)).ReturnsAsync(programme);
            var dateToDisplay = programme.LastDateStarts.HasValue
                ? programme.LastDateStarts.Value.AsGdsDate()
                : programme.EffectiveTo.Value.AsGdsDate();

            var result = Validator.Validate(vacancy, VacancyRuleSet.TrainingExpiryDate);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors.First()
                .ErrorMessage.Should()
                .Be(
                    $"Start date must be on or before {dateToDisplay} as this is the last day for new starters for the training course you have selected. If you don't want to change the start date, you can change the training course");
        }
    }
}