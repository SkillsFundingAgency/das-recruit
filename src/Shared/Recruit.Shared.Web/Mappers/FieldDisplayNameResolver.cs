using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Application.Services;

namespace Esfa.Recruit.Shared.Web.Mappers
{
    public static class FieldDisplayNameResolver
    {
        private const string EmployerLocationAddress = "Employer location address";
        private static readonly Dictionary<string, string> FieldNames = new()
        {
            { FieldIdResolver.ToFieldId(v => v.VacancyReference), "Reference number" },
            { FieldIdResolver.ToFieldId(v => v.EmployerAccountId), "Employer Account Id" },
            { FieldIdResolver.ToFieldId(v => v.AnonymousReason), "Anonymous reason" },
            { FieldIdResolver.ToFieldId(v => v.ApplicationInstructions), "Application process instructions" },
            { FieldIdResolver.ToFieldId(v => v.ApplicationMethod), "Application process" },
            { FieldIdResolver.ToFieldId(v => v.ApplicationUrl), "Application web address" },
            { FieldIdResolver.ToFieldId(v => v.ClosingDate), "Closing date" },
            { FieldIdResolver.ToFieldId(v => v.Description), "What will the apprentice be doing?" },
            { FieldIdResolver.ToFieldId(v => v.DisabilityConfident), "Disability Confident" },
            { FieldIdResolver.ToFieldId(v => v.EmployerContact.Email), "Employer contact email" },
            { FieldIdResolver.ToFieldId(v => v.EmployerContact.Name), "Employer contact name" },
            { FieldIdResolver.ToFieldId(v => v.EmployerContact.Phone), "Employer contact phone" },
            { FieldIdResolver.ToFieldId(v => v.EmployerDescription), "Tell us about your organisation" },
            { FieldIdResolver.ToFieldId(v => v.EmployerLocation.AddressLine1),  EmployerLocationAddress},
            { FieldIdResolver.ToFieldId(v => v.EmployerLocation.AddressLine2), EmployerLocationAddress },
            { FieldIdResolver.ToFieldId(v => v.EmployerLocation.AddressLine3), EmployerLocationAddress },
            { FieldIdResolver.ToFieldId(v => v.EmployerLocation.AddressLine4), EmployerLocationAddress },
            { FieldIdResolver.ToFieldId(v => v.EmployerLocation.Postcode), EmployerLocationAddress },
            { FieldIdResolver.ToFieldId(v => v.EmployerLocations), "Where is this apprenticeship available section" },
            { FieldIdResolver.ToFieldId(v => v.EmployerLocationInformation), "Where is this apprenticeship available section" },
            { FieldIdResolver.ToFieldId(v => v.EmployerName), "Employer name" },
            { FieldIdResolver.ToFieldId(v => v.EmployerWebsiteUrl), "Organisation website" },
            { FieldIdResolver.ToFieldId(v => v.NumberOfPositions), "Number of positions" },
            { FieldIdResolver.ToFieldId(v => v.OutcomeDescription), "What is the expected career progression after this apprenticeship?" },
            { FieldIdResolver.ToFieldId(v => v.ProgrammeId), "Training" },
            { FieldIdResolver.ToFieldId(v => v.Qualifications), "Qualifications" },
            { FieldIdResolver.ToFieldId(v => v.ShortDescription), "Brief overview" },
            { FieldIdResolver.ToFieldId(v => v.Skills), "Skills" },
            { FieldIdResolver.ToFieldId(v => v.StartDate), "Start date" },
            { FieldIdResolver.ToFieldId(v => v.ThingsToConsider), "Things to consider" },
            { FieldIdResolver.ToFieldId(v => v.Title), "Title" },
            { FieldIdResolver.ToFieldId(v => v.TrainingDescription), "What training will the apprentice take and what qualification will the apprentice get at the end?" },
            { FieldIdResolver.ToFieldId(v => v.TrainingProvider.Ukprn) , "Training provider" },
            { FieldIdResolver.ToFieldId(v => v.Wage.WeeklyHours), "Weekly hours" },
            { FieldIdResolver.ToFieldId(v => v.Wage.WorkingWeekDescription), "Working week" },
            { FieldIdResolver.ToFieldId(v => v.Wage.WageAdditionalInformation), "Additional information about pay" },
            { FieldIdResolver.ToFieldId(v => v.Wage.WageType), "What are you going to pay your apprentice" },
            { FieldIdResolver.ToFieldId(v => v.Wage.FixedWageYearlyAmount),"Fixed wage amount" },
            { FieldIdResolver.ToFieldId(v => v.Wage.Duration), "Duration" },
            { FieldIdResolver.ToFieldId(v => v.Wage.DurationUnit), "Duration unit" },
            { FieldIdResolver.ToFieldId(v => v.ProviderContact.Email), "Provider contact email" },
            { FieldIdResolver.ToFieldId(v => v.ProviderContact.Name), "Provider contact name" },
            { FieldIdResolver.ToFieldId(v => v.ProviderContact.Phone), "Provider contact phone" },
            { FieldIdResolver.ToFieldId(v => v.AdditionalQuestion1), "Additional Question 1" },
            { FieldIdResolver.ToFieldId(v => v.AdditionalQuestion2), "Additional Question 2" },
        };
        
        public static string Resolve(string fieldId)
        {
            if (FieldNames.ContainsKey(fieldId))
                return FieldNames[fieldId];

            return fieldId;
        }
    }
}
