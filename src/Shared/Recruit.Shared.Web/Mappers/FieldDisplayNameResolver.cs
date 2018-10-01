using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Application.Services;

namespace Esfa.Recruit.Shared.Web.Mappers
{
    public static class FieldDisplayNameResolver
    {
        private static readonly Dictionary<string, string> FieldNames = new Dictionary<string, string>
        {
            {FieldIdResolver.ToFieldId(v => v.VacancyReference), "Reference number"},
            {FieldIdResolver.ToFieldId(v => v.EmployerAccountId), "Employer Account Id"},
            {FieldIdResolver.ToFieldId(v => v.ApplicationInstructions), "application process instructions"},
            {FieldIdResolver.ToFieldId(v => v.ApplicationMethod), "Application process" },
            {FieldIdResolver.ToFieldId(v => v.ApplicationUrl), "Application web address" },
            {FieldIdResolver.ToFieldId(v => v.ClosingDate), "Closing date" },
            {FieldIdResolver.ToFieldId(v => v.Description), "What does the apprenticeship involve" },
            {FieldIdResolver.ToFieldId(v => v.DisabilityConfident), "Disability Confident" },
            {FieldIdResolver.ToFieldId(v => v.EmployerContactEmail), "Employer contact email" },
            {FieldIdResolver.ToFieldId(v => v.EmployerContactName), "Employer contact name"},
            {FieldIdResolver.ToFieldId(v => v.EmployerContactPhone), "Employer contact phone"},
            {FieldIdResolver.ToFieldId(v => v.EmployerDescription), "Tell us about your organisation"},
            {FieldIdResolver.ToFieldId(v => v.EmployerLocation.AddressLine1), "Employer location address line 1"},
            {FieldIdResolver.ToFieldId(v => v.EmployerLocation.AddressLine2), "Employer location address line 2"},
            {FieldIdResolver.ToFieldId(v => v.EmployerLocation.AddressLine3), "Employer location address line 3"},
            {FieldIdResolver.ToFieldId(v => v.EmployerLocation.AddressLine4), "Employer location address line 4"},
            {FieldIdResolver.ToFieldId(v => v.EmployerLocation.Postcode), "Employer location postcode"},
            {FieldIdResolver.ToFieldId(v => v.EmployerName), "Employer name"},
            {FieldIdResolver.ToFieldId(v => v.EmployerWebsiteUrl), "Employer's website"},
            {FieldIdResolver.ToFieldId(v => v.NumberOfPositions), "Number of positions"},
            {FieldIdResolver.ToFieldId(v => v.OutcomeDescription), "Future prospects"},
            {FieldIdResolver.ToFieldId(v => v.ProgrammeId), "Training"},
            {FieldIdResolver.ToFieldId(v => v.Qualifications), "Qualifications"},
            {FieldIdResolver.ToFieldId(v => v.ShortDescription), "Overview of the role"},
            {FieldIdResolver.ToFieldId(v => v.Skills), "Skills"},
            {FieldIdResolver.ToFieldId(v => v.StartDate), "Start date"},
            {FieldIdResolver.ToFieldId(v => v.ThingsToConsider), "Things to consider"},
            {FieldIdResolver.ToFieldId(v => v.Title), "Title"},
            {FieldIdResolver.ToFieldId(v => v.TrainingDescription), "Training to be provided"},
            {FieldIdResolver.ToFieldId(v => v.TrainingProvider.Ukprn) , "Training provider"},
            {FieldIdResolver.ToFieldId(v => v.Wage.WeeklyHours), "Weekly hours"},
            {FieldIdResolver.ToFieldId(v => v.Wage.WorkingWeekDescription), "Working week"},
            {FieldIdResolver.ToFieldId(v => v.Wage.WageAdditionalInformation), "Additional information about the salary"},
            {FieldIdResolver.ToFieldId(v => v.Wage.WageType),"What is the salary"},
            {FieldIdResolver.ToFieldId(v => v.Wage.FixedWageYearlyAmount),"Fixed wage amount"},
            {FieldIdResolver.ToFieldId(v => v.Wage.Duration),"Duration"},
            {FieldIdResolver.ToFieldId(v => v.Wage.DurationUnit),"Duration unit"}
        };
        
        public static string Resolve(string fieldId)
        {
            if (FieldNames.ContainsKey(fieldId))
                return FieldNames[fieldId];

            return fieldId;
        }
    }
}
