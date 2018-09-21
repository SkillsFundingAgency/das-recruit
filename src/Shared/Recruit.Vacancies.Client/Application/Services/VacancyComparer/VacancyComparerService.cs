using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using SFA.DAS.EAS.Account.Api.Types;

namespace Esfa.Recruit.Vacancies.Client.Application.Services.VacancyComparer
{
    public class VacancyComparerService : IVacancyComparerService
    {
        public VacancyComparerResult Compare(Vacancy a, Vacancy b)
        {
            var fields = new List<VacancyComparerField>
            {
                CompareValue(a, b, v => v.VacancyReference, nameof(Vacancy.VacancyReference)),
                CompareValue(a, b, v => v.EmployerAccountId, nameof(Vacancy.EmployerAccountId)),
                CompareValue(a, b, v => v.ApplicationInstructions, nameof(Vacancy.ApplicationInstructions)),
                CompareValue(a, b, v => v.ApplicationMethod, nameof(Vacancy.ApplicationMethod)),
                CompareValue(a, b, v => v.ApplicationUrl, nameof(Vacancy.ApplicationUrl)),
                CompareValue(a, b, v => v.ClosingDate, nameof(Vacancy.ClosingDate)),
                CompareValue(a, b, v => v.Description, nameof(Vacancy.Description)),
                CompareValue(a, b, v => v.DisabilityConfident, nameof(Vacancy.DisabilityConfident)),
                CompareValue(a, b, v => v.EmployerContactEmail, nameof(Vacancy.EmployerContactEmail)),
                CompareValue(a, b, v => v.EmployerContactName, nameof(Vacancy.EmployerContactName)),
                CompareValue(a, b, v => v.EmployerContactPhone, nameof(Vacancy.EmployerContactPhone)),
                CompareValue(a, b, v => v.EmployerDescription, nameof(Vacancy.EmployerDescription)),
                CompareValue(a, b, v => v.EmployerLocation?.AddressLine1, $"{nameof(Vacancy.EmployerLocation)}.{nameof(Address.AddressLine1)}"),
                CompareValue(a, b, v => v.EmployerLocation?.AddressLine2, $"{nameof(Vacancy.EmployerLocation)}.{nameof(Address.AddressLine2)}"),
                CompareValue(a, b, v => v.EmployerLocation?.AddressLine3, $"{nameof(Vacancy.EmployerLocation)}.{nameof(Address.AddressLine3)}"),
                CompareValue(a, b, v => v.EmployerLocation?.AddressLine4, $"{nameof(Vacancy.EmployerLocation)}.{nameof(Address.AddressLine4)}"),
                CompareValue(a, b, v => v.EmployerLocation?.Postcode, $"{nameof(Vacancy.EmployerLocation)}.{nameof(Address.Postcode)}"),
                CompareValue(a, b, v => v.EmployerName, nameof(Vacancy.EmployerName)),
                CompareValue(a, b, v => v.EmployerWebsiteUrl, nameof(Vacancy.EmployerWebsiteUrl)),
                CompareValue(a, b, v => v.NumberOfPositions, nameof(Vacancy.NumberOfPositions)),
                CompareValue(a, b, v => v.OutcomeDescription, nameof(Vacancy.OutcomeDescription)),
                CompareValue(a, b, v => v.ProgrammeId, nameof(Vacancy.ProgrammeId)),
                CompareList(a, b, v => v.Qualifications, nameof(Vacancy.Qualifications)),
                CompareValue(a, b, v => v.ShortDescription, nameof(Vacancy.ShortDescription)),
                CompareList(a, b, v => v.Skills, nameof(Vacancy.Skills)),
                CompareValue(a, b, v => v.StartDate, nameof(Vacancy.StartDate)),
                CompareValue(a, b, v => v.ThingsToConsider, nameof(Vacancy.ThingsToConsider)),
                CompareValue(a, b, v => v.Title, nameof(Vacancy.Title)),
                CompareValue(a, b, v => v.TrainingDescription, nameof(Vacancy.TrainingDescription)),
                CompareValue(a, b, v => v.TrainingProvider?.Ukprn, $"{nameof(Vacancy.TrainingProvider)}.{nameof(TrainingProvider.Ukprn)}"),
                CompareValue(a, b, v => v.Wage?.WeeklyHours, $"{nameof(Vacancy.Wage)}.{nameof(Wage.WeeklyHours)}"),
                CompareValue(a, b, v => v.Wage?.WorkingWeekDescription, $"{nameof(Vacancy.Wage)}.{nameof(Wage.WorkingWeekDescription)}"),
                CompareValue(a, b, v => v.Wage?.WageAdditionalInformation, $"{nameof(Vacancy.Wage)}.{nameof(Wage.WageAdditionalInformation)}"),
                CompareValue(a, b, v => v.Wage?.WageType, $"{nameof(Vacancy.Wage)}.{nameof(Wage.WageType)}"),
                CompareValue(a, b, v => v.Wage?.FixedWageYearlyAmount, $"{nameof(Vacancy.Wage)}.{nameof(Wage.FixedWageYearlyAmount)}"),
                CompareValue(a, b, v => v.Wage?.Duration, $"{nameof(Vacancy.Wage)}.{nameof(Wage.Duration)}"),
                CompareValue(a, b, v => v.Wage?.DurationUnit, $"{nameof(Vacancy.Wage)}.{nameof(Wage.DurationUnit)}")
            };
            
            return new VacancyComparerResult {Fields = fields };
        }
        
        private static VacancyComparerField CompareValue<T, P>(T a, T b, Func<T, P> valueFunc, string fieldName)
        {
            var aValue = valueFunc(a);
            var bValue = valueFunc(b);

            var areEqual = EqualityComparer<P>.Default.Equals(aValue, bValue);
            
            return new VacancyComparerField(fieldName, areEqual);
        }
        
        private static VacancyComparerField CompareList<T, P>(T a, T b, Func<T, IEnumerable<P>> valueFunc, string fieldName)
        {
            var aValue = valueFunc(a) ?? Enumerable.Empty<P>();
            var bValue = valueFunc(b) ?? Enumerable.Empty<P>();

            var areEqual = aValue.SequenceEqual(bValue);

            return new VacancyComparerField(fieldName, areEqual);
        }
    }
}
