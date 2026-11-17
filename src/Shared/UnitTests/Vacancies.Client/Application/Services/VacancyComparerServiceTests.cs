using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Application.Services.VacancyComparer;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.Services;

public class VacancyComparerServiceTests
{
    private static IEnumerable<object[]> TestCases => new List<object[]>
    {
        new object[] {CreateVacancy(), CreateEmptyVacancy(), false},
        new object[] {CreateVacancy(), CreateChangedVacancy(), false},
        new object[] {CreateVacancy(), CreateVacancy(), true}
    };
        
    [TestCaseSource(nameof(TestCases))]
    public void ShouldCompare(Vacancy a, Vacancy b, bool expectedAreEqual)
    {
        var sut = new VacancyComparerService();

        var result = sut.Compare(a, b);

        result.Fields.Single(f => f.FieldName == FieldIdResolver.ToFieldId(v => v.VacancyReference)).AreEqual.Should().Be(expectedAreEqual);
        result.Fields.Single(f => f.FieldName == FieldIdResolver.ToFieldId(v => v.EmployerAccountId)).AreEqual.Should().Be(expectedAreEqual);
        result.Fields.Single(f => f.FieldName == FieldIdResolver.ToFieldId(v => v.ApplicationInstructions)).AreEqual.Should().Be(expectedAreEqual);
        result.Fields.Single(f => f.FieldName == FieldIdResolver.ToFieldId(v => v.ApplicationMethod)).AreEqual.Should().Be(expectedAreEqual);
        result.Fields.Single(f => f.FieldName == FieldIdResolver.ToFieldId(v => v.ApplicationUrl)).AreEqual.Should().Be(expectedAreEqual);
        result.Fields.Single(f => f.FieldName == FieldIdResolver.ToFieldId(v => v.ClosingDate)).AreEqual.Should().Be(expectedAreEqual);
        result.Fields.Single(f => f.FieldName == FieldIdResolver.ToFieldId(v => v.Description)).AreEqual.Should().Be(expectedAreEqual);
        result.Fields.Single(f => f.FieldName == FieldIdResolver.ToFieldId(v => v.DisabilityConfident)).AreEqual.Should().Be(expectedAreEqual);
        result.Fields.Single(f => f.FieldName == FieldIdResolver.ToFieldId(v => v.EmployerContact.Email)).AreEqual.Should().Be(expectedAreEqual);
        result.Fields.Single(f => f.FieldName == FieldIdResolver.ToFieldId(v => v.EmployerContact.Name)).AreEqual.Should().Be(expectedAreEqual);
        result.Fields.Single(f => f.FieldName == FieldIdResolver.ToFieldId(v => v.EmployerContact.Phone)).AreEqual.Should().Be(expectedAreEqual);
        result.Fields.Single(f => f.FieldName == FieldIdResolver.ToFieldId(v => v.EmployerDescription)).AreEqual.Should().Be(expectedAreEqual);
        result.Fields.Single(f => f.FieldName == FieldIdResolver.ToFieldId(v => v.EmployerLocation.AddressLine1)).AreEqual.Should().Be(expectedAreEqual);
        result.Fields.Single(f => f.FieldName == FieldIdResolver.ToFieldId(v => v.EmployerLocation.AddressLine2)).AreEqual.Should().Be(expectedAreEqual);
        result.Fields.Single(f => f.FieldName == FieldIdResolver.ToFieldId(v => v.EmployerLocation.AddressLine3)).AreEqual.Should().Be(expectedAreEqual);
        result.Fields.Single(f => f.FieldName == FieldIdResolver.ToFieldId(v => v.EmployerLocation.AddressLine4)).AreEqual.Should().Be(expectedAreEqual);
        result.Fields.Single(f => f.FieldName == FieldIdResolver.ToFieldId(v => v.EmployerLocation.Postcode)).AreEqual.Should().Be(expectedAreEqual);
        result.Fields.Single(f => f.FieldName == FieldIdResolver.ToFieldId(v => v.EmployerLocations)).AreEqual.Should().Be(expectedAreEqual);
        result.Fields.Single(f => f.FieldName == FieldIdResolver.ToFieldId(v => v.EmployerLocationInformation)).AreEqual.Should().Be(expectedAreEqual);
        result.Fields.Single(f => f.FieldName == FieldIdResolver.ToFieldId(v => v.EmployerName)).AreEqual.Should().Be(expectedAreEqual);
        result.Fields.Single(f => f.FieldName == FieldIdResolver.ToFieldId(v => v.EmployerWebsiteUrl)).AreEqual.Should().Be(expectedAreEqual);
        result.Fields.Single(f => f.FieldName == FieldIdResolver.ToFieldId(v => v.NumberOfPositions)).AreEqual.Should().Be(expectedAreEqual);
        result.Fields.Single(f => f.FieldName == FieldIdResolver.ToFieldId(v => v.OutcomeDescription)).AreEqual.Should().Be(expectedAreEqual);
        result.Fields.Single(f => f.FieldName == FieldIdResolver.ToFieldId(v => v.ProgrammeId)).AreEqual.Should().Be(expectedAreEqual);
        result.Fields.Single(f => f.FieldName == FieldIdResolver.ToFieldId(v => v.ShortDescription)).AreEqual.Should().Be(expectedAreEqual);
        result.Fields.Single(f => f.FieldName == FieldIdResolver.ToFieldId(v => v.StartDate)).AreEqual.Should().Be(expectedAreEqual);
        result.Fields.Single(f => f.FieldName == FieldIdResolver.ToFieldId(v => v.ThingsToConsider)).AreEqual.Should().Be(expectedAreEqual);
        result.Fields.Single(f => f.FieldName == FieldIdResolver.ToFieldId(v => v.Title)).AreEqual.Should().Be(expectedAreEqual);
        result.Fields.Single(f => f.FieldName == FieldIdResolver.ToFieldId(v => v.TrainingDescription)).AreEqual.Should().Be(expectedAreEqual);
        result.Fields.Single(f => f.FieldName == FieldIdResolver.ToFieldId(v => v.TrainingProvider.Ukprn)).AreEqual.Should().Be(expectedAreEqual);
        result.Fields.Single(f => f.FieldName == FieldIdResolver.ToFieldId(v => v.Wage.WeeklyHours)).AreEqual.Should().Be(expectedAreEqual);
        result.Fields.Single(f => f.FieldName == FieldIdResolver.ToFieldId(v => v.Wage.WorkingWeekDescription)).AreEqual.Should().Be(expectedAreEqual);
        result.Fields.Single(f => f.FieldName == FieldIdResolver.ToFieldId(v => v.Wage.WageAdditionalInformation)).AreEqual.Should().Be(expectedAreEqual);
        result.Fields.Single(f => f.FieldName == FieldIdResolver.ToFieldId(v => v.Wage.WageType)).AreEqual.Should().Be(expectedAreEqual);
        result.Fields.Single(f => f.FieldName == FieldIdResolver.ToFieldId(v => v.Wage.FixedWageYearlyAmount)).AreEqual.Should().Be(expectedAreEqual);
        result.Fields.Single(f => f.FieldName == FieldIdResolver.ToFieldId(v => v.Wage.Duration)).AreEqual.Should().Be(expectedAreEqual);
        result.Fields.Single(f => f.FieldName == FieldIdResolver.ToFieldId(v => v.AdditionalQuestion1)).AreEqual.Should().Be(expectedAreEqual);
        result.Fields.Single(f => f.FieldName == FieldIdResolver.ToFieldId(v => v.AdditionalQuestion2)).AreEqual.Should().Be(expectedAreEqual);
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
            EmployerContact = new ContactDetail
            {
                Email = "employer contact email",
                Name = "employer contact name",
                Phone = "employer contact phone",
            },
            EmployerDescription = "employer description",
            EmployerLocation = new Address
            {
                AddressLine1 = "address line 1",
                AddressLine2 = "address line 2",
                AddressLine3 = "address line 3",
                AddressLine4 = "address line 4",
                Postcode = "post code"
            },
            EmployerLocations = [new Address
            {
                AddressLine1 = "address line 1",
                AddressLine2 = "address line 2",
                AddressLine3 = "address line 3",
                AddressLine4 = "address line 4",
                Postcode = "post code"
            }],
            EmployerLocationInformation = "employer location information",
            EmployerName = "employer name",
            EmployerWebsiteUrl = "employer website",
            NumberOfPositions = 5,
            OutcomeDescription = "outcome descriptions",
            ProgrammeId = "programme id",
            Qualifications = new List<Qualification>
            {
                new Qualification{QualificationType = "qualification type 1", Subject = "subject 1", Grade = "grade 1", Weighting = QualificationWeighting.Desired},
                new Qualification{QualificationType = "qualification type 2", Subject = "subject 2", Grade = "grade 2", Weighting = QualificationWeighting.Essential},
                new Qualification{QualificationType = "qualification type 3", Level = 1, Subject = "subject 2", Grade = "grade 2", Weighting = QualificationWeighting.Essential},
            },
            ShortDescription = "short description",
            Skills = new List<string> { "skill 1", "skill 2" },
            StartDate = new DateTime(),
            ThingsToConsider = "things to consider",
            Title = "title",
            TrainingDescription = "training description",
            TrainingProvider = new TrainingProvider { Ukprn = 1234 },
            Wage = new Wage
            {
                WeeklyHours = 35.5m,
                WorkingWeekDescription = "working week description",
                WageAdditionalInformation = "wage additional information",
                WageType = WageType.FixedWage,
                FixedWageYearlyAmount = 1000.00m,
                Duration = 1,
                DurationUnit = DurationUnit.Month
            },
            AdditionalQuestion1 = "Additional question",
            AdditionalQuestion2 = "Additional question 2",
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
            EmployerContact = new ContactDetail
            {
                Email = "employer contact email CHANGED",
                Name = "employer contact name CHANGED",
                Phone = "employer contact phone CHANGED",
            },
            EmployerDescription = "employer description CHANGED",
            EmployerLocation = new Address
            {
                AddressLine1 = "address line 1 CHANGED",
                AddressLine2 = "address line 2 CHANGED",
                AddressLine3 = "address line 3 CHANGED",
                AddressLine4 = "address line 4 CHANGED",
                Postcode = "post code CHANGED"
            },
            EmployerLocations = [new Address
            {
                AddressLine1 = "address line 1 CHANGED",
                AddressLine2 = "address line 2 CHANGED",
                AddressLine3 = "address line 3 CHANGED",
                AddressLine4 = "address line 4 CHANGED",
                Postcode = "post code CHANGED"
            }],
            EmployerLocationInformation = "employer location information CHANGED",
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
            Skills = new List<string> { "skill 1", "skill 2 CHANGED" },
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
            },
            AdditionalQuestion1 = "Additional question CHANGED",
            AdditionalQuestion2 = "Additional question CHANGED",
        };
    }

    private static Vacancy CreateEmptyVacancy()
    {
        return new Vacancy
        {
            VacancyReference = null,
            EmployerAccountId = null,
            ApplicationInstructions = null,
            ApplicationMethod = null,
            ApplicationUrl = null,
            ClosingDate = null,
            Description = null,
            DisabilityConfident = DisabilityConfident.Yes,
            EmployerContact = null,
            EmployerDescription = null,
            EmployerLocation = null,
            EmployerLocations = null,
            EmployerLocationInformation = null,
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
            Wage = null,
            AdditionalQuestion1 = null,
            AdditionalQuestion2 = null
        };
    }
}