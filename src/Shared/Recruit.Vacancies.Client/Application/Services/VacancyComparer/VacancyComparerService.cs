using System;
using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Application.Services.VacancyComparer;

public class VacancyComparerService : IVacancyComparerService
{
    public VacancyComparerResult Compare(Vacancy a, Vacancy b)
    {
        var fields = new List<VacancyComparerField>
        {
            CompareValue(a, b, v => v.VacancyReference, FieldIdResolver.ToFieldId(v => v.VacancyReference)),
            CompareValue(a, b, v => v.EmployerAccountId, FieldIdResolver.ToFieldId(v => v.EmployerAccountId)),
            CompareValue(a, b, v => v.AnonymousReason, FieldIdResolver.ToFieldId(v => v.AnonymousReason)),
            CompareValue(a, b, v => v.ApplicationInstructions, FieldIdResolver.ToFieldId(v => v.ApplicationInstructions)),
            CompareValue(a, b, v => v.ApplicationMethod, FieldIdResolver.ToFieldId(v => v.ApplicationMethod)),
            CompareValue(a, b, v => v.ApplicationUrl, FieldIdResolver.ToFieldId(v => v.ApplicationUrl)),
            CompareValue(a, b, v => v.ClosingDate, FieldIdResolver.ToFieldId(v => v.ClosingDate)),
            CompareValue(a, b, v => v.Description, FieldIdResolver.ToFieldId(v => v.Description)),
            CompareValue(a, b, v => v.DisabilityConfident, FieldIdResolver.ToFieldId(v => v.DisabilityConfident)),
            CompareValue(a, b, v => v.EmployerContact?.Email, FieldIdResolver.ToFieldId(v => v.EmployerContact.Email)),
            CompareValue(a, b, v => v.EmployerContact?.Name, FieldIdResolver.ToFieldId(v => v.EmployerContact.Name)),
            CompareValue(a, b, v => v.EmployerContact?.Phone, FieldIdResolver.ToFieldId(v => v.EmployerContact.Phone)),
            CompareValue(a, b, v => v.EmployerDescription, FieldIdResolver.ToFieldId(v => v.EmployerDescription)),
            CompareValue(a, b, v => v.EmployerLocation?.AddressLine1,  FieldIdResolver.ToFieldId(v => v.EmployerLocation.AddressLine1)),
            CompareValue(a, b, v => v.EmployerLocation?.AddressLine2, FieldIdResolver.ToFieldId(v => v.EmployerLocation.AddressLine2)),
            CompareValue(a, b, v => v.EmployerLocation?.AddressLine3, FieldIdResolver.ToFieldId(v => v.EmployerLocation.AddressLine3)),
            CompareValue(a, b, v => v.EmployerLocation?.AddressLine4, FieldIdResolver.ToFieldId(v => v.EmployerLocation.AddressLine4)),
            CompareValue(a, b, v => v.EmployerLocation?.Postcode, FieldIdResolver.ToFieldId(v => v.EmployerLocation.Postcode)),
            CompareList(a, b, v => v.EmployerLocations,  FieldIdResolver.ToFieldId(v => v.EmployerLocations)),
            CompareValue(a, b, v => v.EmployerLocationInformation,  FieldIdResolver.ToFieldId(v => v.EmployerLocationInformation)),
            CompareValue(a, b, v => v.EmployerName, FieldIdResolver.ToFieldId(v => v.EmployerName)),
            CompareValue(a, b, v => v.EmployerWebsiteUrl, FieldIdResolver.ToFieldId(v => v.EmployerWebsiteUrl)),
            CompareValue(a, b, v => v.NumberOfPositions, FieldIdResolver.ToFieldId(v => v.NumberOfPositions)),
            CompareValue(a, b, v => v.OutcomeDescription, FieldIdResolver.ToFieldId(v => v.OutcomeDescription)),
            CompareValue(a, b, v => v.ProgrammeId, FieldIdResolver.ToFieldId(v => v.ProgrammeId)),
            CompareValue(a, b, v => v.ProviderContact?.Email, FieldIdResolver.ToFieldId(v => v.ProviderContact.Email)),
            CompareValue(a, b, v => v.ProviderContact?.Name, FieldIdResolver.ToFieldId(v => v.ProviderContact.Name)),
            CompareValue(a, b, v => v.ProviderContact?.Phone, FieldIdResolver.ToFieldId(v => v.ProviderContact.Phone)),
            CompareList(a, b, v => v.Qualifications, FieldIdResolver.ToFieldId(v => v.Qualifications)),
            CompareValue(a, b, v => v.ShortDescription, FieldIdResolver.ToFieldId(v => v.ShortDescription)),
            CompareList(a, b, v => v.Skills, FieldIdResolver.ToFieldId(v => v.Skills)),
            CompareValue(a, b, v => v.StartDate, FieldIdResolver.ToFieldId(v => v.StartDate)),
            CompareValue(a, b, v => v.ThingsToConsider, FieldIdResolver.ToFieldId(v => v.ThingsToConsider)),
            CompareValue(a, b, v => v.Title, FieldIdResolver.ToFieldId(v => v.Title)),
            CompareValue(a, b, v => v.TrainingDescription, FieldIdResolver.ToFieldId(v => v.TrainingDescription)),
            CompareValue(a, b, v => v.TrainingProvider?.Ukprn, FieldIdResolver.ToFieldId(v => v.TrainingProvider.Ukprn)),
            CompareValue(a, b, v => v.Wage?.WeeklyHours, FieldIdResolver.ToFieldId(v => v.Wage.WeeklyHours)),
            CompareValue(a, b, v => v.Wage?.WorkingWeekDescription, FieldIdResolver.ToFieldId(v => v.Wage.WorkingWeekDescription)),
            CompareValue(a, b, v => v.Wage?.WageAdditionalInformation, FieldIdResolver.ToFieldId(v => v.Wage.WageAdditionalInformation)),
            CompareValue(a, b, v => v.Wage?.WageType, FieldIdResolver.ToFieldId(v => v.Wage.WageType)),
            CompareValue(a, b, v => v.Wage?.FixedWageYearlyAmount, FieldIdResolver.ToFieldId(v => v.Wage.FixedWageYearlyAmount)),
            CompareValue(a, b, v => v.Wage?.Duration, FieldIdResolver.ToFieldId(v => v.Wage.Duration)),
            CompareValue(a, b, v => v.Wage?.DurationUnit, FieldIdResolver.ToFieldId(v => v.Wage.DurationUnit)),
            CompareValue(a, b, v => v.AdditionalQuestion1, FieldIdResolver.ToFieldId(v => v.AdditionalQuestion1)),
            CompareValue(a, b, v => v.AdditionalQuestion2, FieldIdResolver.ToFieldId(v => v.AdditionalQuestion2))
        };
            
        return new VacancyComparerResult {Fields = fields };
    }

    private static VacancyComparerField CompareValue<T, P>(T a, T b, Func<T, P> valueFunc, string fieldName)
    {
        var aValue = valueFunc(a);
        var bValue = valueFunc(b);

        bool areEqual = EqualityComparer<P>.Default.Equals(aValue, bValue);
            
        return new VacancyComparerField(fieldName, areEqual);
    }
        
    private static VacancyComparerField CompareList<T, P>(T a, T b, Func<T, IEnumerable<P>> valueFunc, string fieldName)
    {
        var aValue = valueFunc(a) ?? [];
        var bValue = valueFunc(b) ?? [];

        bool areEqual = aValue.SequenceEqual(bValue);

        return new VacancyComparerField(fieldName, areEqual);
    }
}