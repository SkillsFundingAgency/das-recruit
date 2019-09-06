using System;
using System.Linq;
using AutoFixture;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Extensions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Vacancy;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.ApprenticeshipProgrammes;
using FluentAssertions;
using Moq;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.Extensions
{
    public class VacancyExtensionsTests
    {
        private const string DocumentId = "doc_id";

        private DateTime _now = DateTime.UtcNow;
        private ApprenticeshipProgramme _programme = new ApprenticeshipProgramme 
        {
            Level = ApprenticeshipLevel.Advanced,
            ApprenticeshipType = TrainingType.Standard,
            EducationLevelNumber = 7
        };

        [Fact]
        public void ToVacancyProjectionBase_ShouldMapLiveVacancy()
        {
            
            var v = new Fixture().Create<Vacancy>();
            v.EmployerNameOption = EmployerNameOption.TradingName;

            var mockTimeProvider = new Mock<ITimeProvider>();
            mockTimeProvider.Setup(t => t.Now).Returns(_now);
            
            var p = v.ToVacancyProjectionBase<LiveVacancy>(_programme, () => DocumentId, mockTimeProvider.Object);

            AssertCommonProperties(v, p);

            p.EmployerLocation.AddressLine1.Should().Be(v.EmployerLocation.AddressLine1);
            p.EmployerLocation.AddressLine2.Should().Be(v.EmployerLocation.AddressLine2);
            p.EmployerLocation.AddressLine3.Should().Be(v.EmployerLocation.AddressLine3);
            p.EmployerLocation.AddressLine4.Should().Be(v.EmployerLocation.AddressLine4);
            p.EmployerLocation.Postcode.Should().Be(v.EmployerLocation.Postcode);

            p.EmployerWebsiteUrl.Should().Be(v.EmployerWebsiteUrl);
        }

        [Fact]
        public void ToVacancyProjectionBase_ShouldMapClosedVacancy()
        {

            var v = new Fixture().Create<Vacancy>();
            v.EmployerNameOption = EmployerNameOption.TradingName;

            var mockTimeProvider = new Mock<ITimeProvider>();
            mockTimeProvider.Setup(t => t.Now).Returns(_now);

            var p = v.ToVacancyProjectionBase<ClosedVacancy>(_programme, () => DocumentId, mockTimeProvider.Object);

            AssertCommonProperties(v, p);

            p.EmployerLocation.AddressLine1.Should().Be(v.EmployerLocation.AddressLine1);
            p.EmployerLocation.AddressLine2.Should().Be(v.EmployerLocation.AddressLine2);
            p.EmployerLocation.AddressLine3.Should().Be(v.EmployerLocation.AddressLine3);
            p.EmployerLocation.AddressLine4.Should().Be(v.EmployerLocation.AddressLine4);
            p.EmployerLocation.Postcode.Should().Be(v.EmployerLocation.Postcode);

            p.EmployerWebsiteUrl.Should().Be(v.EmployerWebsiteUrl);
        }

        [Fact]
        public void ToVacancyProjectionBase_ShouldMapAnonymousVacancy()
        {

            var v = new Fixture().Create<Vacancy>();
            v.EmployerNameOption = EmployerNameOption.Anonymous;
            v.EmployerLocation.Postcode = "SW1A 2AA";

            var mockTimeProvider = new Mock<ITimeProvider>();
            mockTimeProvider.Setup(t => t.Now).Returns(_now);

            var p = v.ToVacancyProjectionBase<LiveVacancy>(_programme, () => DocumentId, mockTimeProvider.Object);

            AssertCommonProperties(v, p);

            p.EmployerLocation.AddressLine1.Should().BeNull();
            p.EmployerLocation.AddressLine2.Should().BeNull();
            p.EmployerLocation.AddressLine3.Should().BeNull();
            p.EmployerLocation.AddressLine4.Should().BeNull();
            p.EmployerLocation.Postcode.Should().Be("SW1A");

            p.EmployerWebsiteUrl.Should().BeNull();
        }

        private  void AssertCommonProperties(Vacancy v, VacancyProjectionBase p)
        {
            p.Id.Should().Be(DocumentId);
            p.LastUpdated.Should().Be(_now);
            p.VacancyId.Should().Be(v.Id);

            p.ApplicationInstructions.Should().Be(v.ApplicationInstructions);
            p.ApplicationMethod.Should().Be(v.ApplicationMethod.ToString());
            p.ApplicationUrl.Should().Be(v.ApplicationUrl);
            p.ClosingDate.Should().Be(v.ClosingDate.Value);
            p.Description.Should().Be(v.Description);
            p.DisabilityConfident.Should().Be(v.DisabilityConfident);
            p.EmployerContactEmail.Should().Be(v.EmployerContact.Email);
            p.EmployerContactName.Should().Be(v.EmployerContact.Name);
            p.EmployerContactPhone.Should().Be(v.EmployerContact.Phone);
            p.ProviderContactEmail.Should().Be(v.ProviderContact.Email);
            p.ProviderContactName.Should().Be(v.ProviderContact.Name);
            p.ProviderContactPhone.Should().Be(v.ProviderContact.Phone);
            p.EmployerDescription.Should().Be(v.EmployerDescription);
            p.EmployerName.Should().Be(v.EmployerName);
            p.IsAnonymous.Should().Be(v.IsAnonymous);
            p.LiveDate.Should().Be(v.LiveDate.Value);
            p.NumberOfPositions.Should().Be(v.NumberOfPositions.Value);
            p.OutcomeDescription.Should().Be(v.OutcomeDescription);
            p.ProgrammeId.Should().Be(v.ProgrammeId);
            p.ProgrammeLevel.Should().Be(_programme.Level.ToString());
            p.ProgrammeType.Should().Be(_programme.ApprenticeshipType.ToString());
            p.EducationLevelNumber.Should().Be(_programme.EducationLevelNumber);
            p.ShortDescription.Should().Be(v.ShortDescription);
            p.StartDate.Should().Be(v.StartDate.Value);
            p.ThingsToConsider.Should().Be(v.ThingsToConsider);
            p.Title.Should().Be(v.Title);
            p.TrainingDescription.Should().Be(v.TrainingDescription);
            p.VacancyReference.Should().Be(v.VacancyReference.Value);

            p.EmployerLocation.Latitude.Should().Be(v.EmployerLocation.Latitude);
            p.EmployerLocation.Longitude.Should().Be(v.EmployerLocation.Longitude);

            var projectionSkills = p.Skills.ToList();
            for (var i = 0; i < projectionSkills.Count; i++)
            {
                projectionSkills[0].Should().Be(v.Skills[0]);
            }

            var projectionQualifications = p.Qualifications.ToList();
            for (var i = 0; i < p.Qualifications.Count(); i++)
            {
                projectionQualifications[i].QualificationType.Should().Be(v.Qualifications[i].QualificationType);
            }

            p.TrainingProvider.Name.Should().Be(v.TrainingProvider.Name);
            p.TrainingProvider.Ukprn.Should().Be(v.TrainingProvider.Ukprn);

            p.Wage.Duration.Should().Be(v.Wage.Duration);
            p.Wage.DurationUnit.Should().Be(v.Wage.DurationUnit.Value.ToString());
            p.Wage.FixedWageYearlyAmount.Should().Be(v.Wage.FixedWageYearlyAmount);
            p.Wage.WageAdditionalInformation.Should().Be(v.Wage.WageAdditionalInformation);
            p.Wage.WageType.Should().Be(v.Wage.WageType.ToString());
            p.Wage.WeeklyHours.Should().Be(v.Wage.WeeklyHours);
            p.Wage.WorkingWeekDescription.Should().Be(v.Wage.WorkingWeekDescription);
        }
    }
}
