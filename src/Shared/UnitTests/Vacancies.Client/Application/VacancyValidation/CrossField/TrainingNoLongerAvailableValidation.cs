using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.UnitTests.Vacancies.Client.Application.VacancyValidation.CrossField;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.VacancyValidation.CrossField
{
    public class TrainingNoLongerAvailableValidation : VacancyValidationTestsBase
    {
        [Fact]
        public void Training_Is_Valid_If_Last_Date_For_Starts_Is_In_The_Future()
        {
            var mockTimeProvider = new Mock<ITimeProvider>();
            mockTimeProvider.Setup(x => x.Now).Returns(DateTime.UtcNow.AddDays(-10));
            TimeProvider = mockTimeProvider.Object;
            var programmes = new List<IApprenticeshipProgramme>
            {
                new TestApprenticeshipProgramme {Id = "123", LastDateStarts = DateTime.UtcNow.AddDays(12) }
            };
            MockApprenticeshipProgrammeProvider.Setup(x => x.GetApprenticeshipProgrammesAsync(false)).ReturnsAsync(programmes);
            var vacancy = new Vacancy
            {
                ProgrammeId = "123",
                StartDate = DateTime.UtcNow.AddDays(2)
            };
         
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
            var programmes = new List<IApprenticeshipProgramme>
            {
                new TestApprenticeshipProgramme {Id = "123", LastDateStarts = DateTime.UtcNow.AddDays(7) }
            };
            MockApprenticeshipProgrammeProvider.Setup(x => x.GetApprenticeshipProgrammesAsync(false)).ReturnsAsync(programmes);
            var vacancy = new Vacancy
            {
                ProgrammeId = "123",
                StartDate = DateTime.UtcNow.AddDays(10)
            };
         
            var result = Validator.Validate(vacancy, VacancyRuleSet.TrainingExpiryDate);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors.FirstOrDefault().ErrorMessage.Should().Be("The training course you have selected is no longer available. You can select a new course or create a new advert.");
        }
        
        [Fact]
        public void Training_Is_Not_Valid_If_Effective_To_Date_Is_In_The_Past()
        {
            var mockTimeProvider = new Mock<ITimeProvider>();
            mockTimeProvider.Setup(x => x.Now).Returns(DateTime.UtcNow.AddDays(8));
            TimeProvider = mockTimeProvider.Object;
            var programmes = new List<IApprenticeshipProgramme>
            {
                new TestApprenticeshipProgramme {Id = "123", EffectiveTo = DateTime.UtcNow.AddDays(7) }
            };
            MockApprenticeshipProgrammeProvider.Setup(x => x.GetApprenticeshipProgrammesAsync(false)).ReturnsAsync(programmes);
            var vacancy = new Vacancy
            {
                ProgrammeId = "123",
                StartDate = DateTime.UtcNow.AddDays(10)
            };
         
            var result = Validator.Validate(vacancy, VacancyRuleSet.TrainingExpiryDate);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors.FirstOrDefault().ErrorMessage.Should().Be("The training course you have selected is no longer available. You can select a new course or create a new advert.");
        }

        [Fact]
        public void Training_Is_Not_Valid_If_No_Longer_Available()
        {
            var mockTimeProvider = new Mock<ITimeProvider>();
            mockTimeProvider.Setup(x => x.Now).Returns(DateTime.UtcNow.AddDays(8));
            TimeProvider = mockTimeProvider.Object;
            var programmes = new List<IApprenticeshipProgramme>
            {
                new TestApprenticeshipProgramme {Id = "1234", EffectiveTo = DateTime.UtcNow.AddDays(7) }
            };
            MockApprenticeshipProgrammeProvider.Setup(x => x.GetApprenticeshipProgrammesAsync(false)).ReturnsAsync(programmes);
            var vacancy = new Vacancy
            {
                ProgrammeId = "123",
                StartDate = DateTime.UtcNow.AddDays(10)
            };
         
            var result = Validator.Validate(vacancy, VacancyRuleSet.TrainingExpiryDate);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors.FirstOrDefault().ErrorMessage.Should().Be("The training course you have selected is no longer available. You can select a new course or create a new advert.");
        }
    }
}