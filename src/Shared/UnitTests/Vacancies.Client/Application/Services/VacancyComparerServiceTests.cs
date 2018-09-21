using System;
using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Application.Services.VacancyComparer;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.Services
{
    
    public class VacancyComparerServiceTests
    {
        public static class ComparisonDataSource
        {
            public static IEnumerable<object[]> TestData => new List<object[]>
            {
                new object[] {CreateVacancy(), CreateEmptyVacancy(), false},
                new object[] {CreateVacancy(), CreateChangedVacancy(), false},
                new object[] {CreateVacancy(), CreateVacancy(), true}
            };
        }

        [Theory]
        [MemberData(nameof(ComparisonDataSource.TestData), MemberType = typeof(ComparisonDataSource))]
        public void ShouldCompare(Vacancy a, Vacancy b, bool expectedAreEqual)
        {
            
            var sut = new VacancyComparerService();

            var result = sut.Compare(a, b);

            result.Fields.Single(f => f.FieldName == nameof(Vacancy.VacancyReference)).AreEqual.Should().Be(expectedAreEqual);
            result.Fields.Single(f => f.FieldName == nameof(Vacancy.EmployerAccountId)).AreEqual.Should().Be(expectedAreEqual);
            result.Fields.Single(f => f.FieldName == nameof(Vacancy.ApplicationInstructions)).AreEqual.Should().Be(expectedAreEqual);
            result.Fields.Single(f => f.FieldName == nameof(Vacancy.ApplicationMethod)).AreEqual.Should().Be(expectedAreEqual);
            result.Fields.Single(f => f.FieldName == nameof(Vacancy.ApplicationUrl)).AreEqual.Should().Be(expectedAreEqual);
            result.Fields.Single(f => f.FieldName == nameof(Vacancy.ClosingDate)).AreEqual.Should().Be(expectedAreEqual);
            result.Fields.Single(f => f.FieldName == nameof(Vacancy.Description)).AreEqual.Should().Be(expectedAreEqual);
            result.Fields.Single(f => f.FieldName == nameof(Vacancy.DisabilityConfident)).AreEqual.Should().Be(expectedAreEqual);
            result.Fields.Single(f => f.FieldName == nameof(Vacancy.EmployerContactEmail)).AreEqual.Should().Be(expectedAreEqual);
            result.Fields.Single(f => f.FieldName == nameof(Vacancy.EmployerContactName)).AreEqual.Should().Be(expectedAreEqual);
            result.Fields.Single(f => f.FieldName == nameof(Vacancy.EmployerContactPhone)).AreEqual.Should().Be(expectedAreEqual);
            result.Fields.Single(f => f.FieldName == nameof(Vacancy.EmployerDescription)).AreEqual.Should().Be(expectedAreEqual);
            result.Fields.Single(f => f.FieldName == $"{nameof(Vacancy.EmployerLocation)}.{nameof(Address.AddressLine1)}").AreEqual.Should().Be(expectedAreEqual);
            result.Fields.Single(f => f.FieldName == $"{nameof(Vacancy.EmployerLocation)}.{nameof(Address.AddressLine2)}").AreEqual.Should().Be(expectedAreEqual);
            result.Fields.Single(f => f.FieldName == $"{nameof(Vacancy.EmployerLocation)}.{nameof(Address.AddressLine3)}").AreEqual.Should().Be(expectedAreEqual);
            result.Fields.Single(f => f.FieldName == $"{nameof(Vacancy.EmployerLocation)}.{nameof(Address.AddressLine4)}").AreEqual.Should().Be(expectedAreEqual);
            result.Fields.Single(f => f.FieldName == $"{nameof(Vacancy.EmployerLocation)}.{nameof(Address.Postcode)}").AreEqual.Should().Be(expectedAreEqual);
            result.Fields.Single(f => f.FieldName == nameof(Vacancy.EmployerName)).AreEqual.Should().Be(expectedAreEqual);
            result.Fields.Single(f => f.FieldName == nameof(Vacancy.EmployerWebsiteUrl)).AreEqual.Should().Be(expectedAreEqual);
            result.Fields.Single(f => f.FieldName == nameof(Vacancy.NumberOfPositions)).AreEqual.Should().Be(expectedAreEqual);
            result.Fields.Single(f => f.FieldName == nameof(Vacancy.OutcomeDescription)).AreEqual.Should().Be(expectedAreEqual);
            result.Fields.Single(f => f.FieldName == nameof(Vacancy.ProgrammeId)).AreEqual.Should().Be(expectedAreEqual);
            result.Fields.Single(f => f.FieldName == nameof(Vacancy.ShortDescription)).AreEqual.Should().Be(expectedAreEqual);
            result.Fields.Single(f => f.FieldName == nameof(Vacancy.StartDate)).AreEqual.Should().Be(expectedAreEqual);
            result.Fields.Single(f => f.FieldName == nameof(Vacancy.ThingsToConsider)).AreEqual.Should().Be(expectedAreEqual);
            result.Fields.Single(f => f.FieldName == nameof(Vacancy.Title)).AreEqual.Should().Be(expectedAreEqual);
            result.Fields.Single(f => f.FieldName == nameof(Vacancy.TrainingDescription)).AreEqual.Should().Be(expectedAreEqual);
            result.Fields.Single(f => f.FieldName == $"{nameof(Vacancy.TrainingProvider)}.{nameof(TrainingProvider.Ukprn)}").AreEqual.Should().Be(expectedAreEqual);
            result.Fields.Single(f => f.FieldName == $"{nameof(Vacancy.Wage)}.{nameof(Wage.WeeklyHours)}").AreEqual.Should().Be(expectedAreEqual);
            result.Fields.Single(f => f.FieldName == $"{nameof(Vacancy.Wage)}.{nameof(Wage.WorkingWeekDescription)}").AreEqual.Should().Be(expectedAreEqual);
            result.Fields.Single(f => f.FieldName == $"{nameof(Vacancy.Wage)}.{nameof(Wage.WageAdditionalInformation)}").AreEqual.Should().Be(expectedAreEqual);
            result.Fields.Single(f => f.FieldName == $"{nameof(Vacancy.Wage)}.{nameof(Wage.WageType)}").AreEqual.Should().Be(expectedAreEqual);
            result.Fields.Single(f => f.FieldName == $"{nameof(Vacancy.Wage)}.{nameof(Wage.FixedWageYearlyAmount)}").AreEqual.Should().Be(expectedAreEqual);
            result.Fields.Single(f => f.FieldName == $"{nameof(Vacancy.Wage)}.{nameof(Wage.Duration)}").AreEqual.Should().Be(expectedAreEqual);
        }
        
        private static Vacancy CreateVacancy()
        {
            return new Vacancy
            {
                VacancyReference = 123456,
                EmployerAccountId = "employer account id",
                ApplicationInstructions = "application instructions",
                ApplicationMethod = ApplicationMethod.ThroughExternalApplicationSite,
                ApplicationUrl = "application url",
                ClosingDate = new DateTime(),
                Description = "description",
                DisabilityConfident = DisabilityConfident.No,
                EmployerContactEmail = "employer contact email",
                EmployerContactName = "employer contact name",
                EmployerContactPhone = "employer contact phone",
                EmployerDescription = "employer description",
                EmployerLocation = new Address
                {
                    AddressLine1 = "address line 1",
                    AddressLine2 = "address line 2",
                    AddressLine3 = "address line 3",
                    AddressLine4 = "address line 4",
                    Postcode = "post code"
                },
                EmployerName = "employer name",
                EmployerWebsiteUrl = "employer website",
                NumberOfPositions = 5,
                OutcomeDescription = "outcome descriptions",
                ProgrammeId = "programme id",
                Qualifications = new List<Qualification>
                {
                    new Qualification{QualificationType = "qualification type 1", Subject = "subject 1", Grade = "grade 1", Weighting = QualificationWeighting.Desired},
                    new Qualification{QualificationType = "qualification type 2", Subject = "subject 2", Grade = "grade 2", Weighting = QualificationWeighting.Essential},
                },
                ShortDescription = "short description",
                Skills = new List<string> { "skill 1", "skill 2" },
                StartDate = new DateTime(),
                ThingsToConsider = "things to consider",
                Title = "title",
                TrainingDescription = "training description",
                TrainingProvider = new TrainingProvider { Ukprn = 1234},
                Wage = new Wage
                {
                    WeeklyHours = 35.5m,
                    WorkingWeekDescription = "working week description",
                    WageAdditionalInformation = "wage additional information",
                    WageType = WageType.FixedWage,
                    FixedWageYearlyAmount = 1000.00m,
                    Duration = 1,
                    DurationUnit = DurationUnit.Month
                }
            };
        }

        private static Vacancy CreateChangedVacancy()
        {
            return new Vacancy
            {
                VacancyReference = 1234567,
                EmployerAccountId = "employer account id CHANGED",
                ApplicationInstructions = "application instructions CHANGED",
                ApplicationMethod = ApplicationMethod.ThroughFindAnApprenticeship,
                ApplicationUrl = "application url CHANGED",
                ClosingDate = DateTime.MaxValue,
                Description = "description CHANGED",
                DisabilityConfident = DisabilityConfident.Yes,
                EmployerContactEmail = "employer contact email CHANGED",
                EmployerContactName = "employer contact name CHANGED",
                EmployerContactPhone = "employer contact phone CHANGED",
                EmployerDescription = "employer description CHANGED",
                EmployerLocation = new Address
                {
                    AddressLine1 = "address line 1 CHANGED",
                    AddressLine2 = "address line 2 CHANGED",
                    AddressLine3 = "address line 3 CHANGED",
                    AddressLine4 = "address line 4 CHANGED",
                    Postcode = "post code CHANGED"
                },
                EmployerName = "employer name CHANGED",
                EmployerWebsiteUrl = "employer website CHANGED",
                NumberOfPositions = 6,
                OutcomeDescription = "outcome descriptions CHANGED",
                ProgrammeId = "programme id CHANGED",
                Qualifications = new List<Qualification>
                {
                    new Qualification{QualificationType = "qualification type 1", Subject = "subject 1", Grade = "grade 1", Weighting = QualificationWeighting.Desired},
                    new Qualification{QualificationType = "qualification type 2", Subject = "subject 2", Grade = "grade 2 CHANGED", Weighting = QualificationWeighting.Essential},
                },
                ShortDescription = "short description CHANGED",
                Skills = new List<string> { "skill 1", "skill 2 CHANGED"},
                StartDate = DateTime.MaxValue,
                ThingsToConsider = "things to consider CHANGED",
                Title = "title CHANGED",
                TrainingDescription = "training description CHANGED",
                TrainingProvider = new TrainingProvider { Ukprn = 12345 },
                Wage = new Wage
                {
                    WeeklyHours = 36.5m,
                    WorkingWeekDescription = "working week description CHANGED",
                    WageAdditionalInformation = "wage additional information CHANGED",
                    WageType = WageType.NationalMinimumWage,
                    FixedWageYearlyAmount = 2000.00m,
                    Duration = 2,
                    DurationUnit = DurationUnit.Year
                }
            };
        }

        private static Vacancy CreateEmptyVacancy()
        {
            return new Vacancy
            {
                VacancyReference = null,
                EmployerAccountId = null,
                ApplicationInstructions =null,
                ApplicationMethod = null,
                ApplicationUrl = null,
                ClosingDate = null,
                Description = null,
                DisabilityConfident = DisabilityConfident.Yes,
                EmployerContactEmail = null,
                EmployerContactName = null,
                EmployerContactPhone = null,
                EmployerDescription = null,
                EmployerLocation = null,
                EmployerName = null,
                EmployerWebsiteUrl = null,
                NumberOfPositions = null,
                OutcomeDescription = null,
                ProgrammeId = null,
                Qualifications = null,
                ShortDescription = null,
                StartDate = null,
                ThingsToConsider = null,
                Title = null,
                TrainingDescription = null,
                TrainingProvider = null,
                Wage = null
            };
        }
    }
}
