using System;
using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;

namespace Esfa.Recruit.Qa.Web.ViewModels
{
    public class 
        ReviewViewModel : ReviewEditModel
    {
        private const string CssFieldChanged = "field-changed";

        public bool IsEditable { get; internal set; }
        public string Title { get; internal set; }
        public string EmployerName { get; internal set; }
        public string ShortDescription { get; internal set; }
        public string ClosingDate { get; internal set; }
        public string ApplicationInstructions { get; internal set; }
        public ApplicationMethod ApplicationMethod { get; internal set; }
        public string ApplicationUrl { get; internal set; }
        public string ContactName { get; internal set; }
        public string ContactEmail { get; internal set; }
        public string ContactTelephone { get; internal set; }
        public string EmployerDescription { get; internal set; }
        public string EmployerWebsiteUrl { get; internal set; }
        public IEnumerable<string> EmployerAddressElements { get; internal set; }
        public bool IsDisabilityConfident { get; set; }
        public string NumberOfPositionsCaption { get; internal set; }
        public string OutcomeDescription { get; internal set; }
        public string HoursPerWeek { get; internal set; }
        public string PossibleStartDate { get; internal set; }
        public string ProviderName { get; internal set; }
        public string ThingsToConsider { get; internal set; }
        public string TrainingDescription { get; internal set; }
        public string VacancyDescription { get; internal set; }
        public string VacancyReferenceNumber { get; internal set; }
        public string TrainingTitle { get; internal set; }
        public string TrainingType { get; internal set; }
        public string TrainingLevel { get; internal set; }
        public string ExpectedDuration { get; internal set; }
        public string WageInfo { get; internal set; }
        public string WorkingWeekDescription { get; internal set; }
        public string MapUrl { get; internal set; }
        public IEnumerable<string> Qualifications { get; internal set; }
        public IEnumerable<string> Skills { get; internal set; }
        public string WageText { get; internal set; }
        public string SubmittedByName { get; internal set; }
        public string SubmittedByEmail { get; internal set; }
        public int VacancyReviewsApprovedCount { get; internal set; }
        public int VacancyReviewsApprovedFirstTimeCount { get; internal set; }
        public DateTime SubmittedDate { get; internal set; }
        public ReviewHistoriesViewModel ReviewHistories { get; internal set; }
        public IEnumerable<FieldIdentifierViewModel> FieldIdentifiers { get; set; }
        public bool IsResubmission { get; set; }

        public bool IsFirstSubmission => IsResubmission == false;
        public bool HasChangedFields => FieldIdentifiers.Any(f => f.FieldValueHasChanged);
        public bool HasSpecifiedThroughFaaApplicationMethod => ApplicationMethod == ApplicationMethod.ThroughFindAnApprenticeship;
        public bool HasSpecifiedThroughExternalApplicationSiteMethod => ApplicationMethod == ApplicationMethod.ThroughExternalApplicationSite;
        public bool HasApplicationInstructions => string.IsNullOrEmpty(ApplicationInstructions) == false;
        public bool HasApplicationUrl => string.IsNullOrEmpty(ApplicationUrl) == false;
        public bool HasPreviouslySubmitted => VacancyReviewsApprovedCount > 0;
        public bool HasNotPreviouslySubmitted => HasPreviouslySubmitted == false;
        public string SubmittedDateTime => SubmittedDate.ToLocalTime().AsGdsDateTime();
        public bool IsNotDisabilityConfident => IsDisabilityConfident == false;

        public string ApplicationInstructionsClass => GetFieldIdentifierCssClass(VacancyReview.FieldIdentifiers.ApplicationInstructions);
        public string ApplicationMethodClass => GetFieldIdentifierCssClass(VacancyReview.FieldIdentifiers.ApplicationMethod);
        public string ApplicationUrlClass => GetFieldIdentifierCssClass(VacancyReview.FieldIdentifiers.ApplicationUrl);
        public string ClosingDateClass => GetFieldIdentifierCssClass(VacancyReview.FieldIdentifiers.ClosingDate);
        public string EmployerContactClass => GetFieldIdentifierCssClass(VacancyReview.FieldIdentifiers.EmployerContact);
        public string DisabilityConfidentClass => GetFieldIdentifierCssClass(VacancyReview.FieldIdentifiers.DisabilityConfident);
        public string EmployerAddressClass => GetFieldIdentifierCssClass(VacancyReview.FieldIdentifiers.EmployerAddress);
        public string EmployerDescriptionClass => GetFieldIdentifierCssClass(VacancyReview.FieldIdentifiers.EmployerDescription);
        public string EmployerWebsiteUrlClass => GetFieldIdentifierCssClass(VacancyReview.FieldIdentifiers.EmployerWebsiteUrl);
        public string ExpectedDurationClass => GetFieldIdentifierCssClass(VacancyReview.FieldIdentifiers.ExpectedDuration);
        public string NumberOfPositionsClass => GetFieldIdentifierCssClass(VacancyReview.FieldIdentifiers.NumberOfPositions);
        public string OutcomeDescriptionClass => GetFieldIdentifierCssClass(VacancyReview.FieldIdentifiers.OutcomeDescription);
        public string PossibleStartDateClass => GetFieldIdentifierCssClass(VacancyReview.FieldIdentifiers.PossibleStartDate);
        public string ProviderClass => GetFieldIdentifierCssClass(VacancyReview.FieldIdentifiers.Provider);
        public string QualificationsClass => GetFieldIdentifierCssClass(VacancyReview.FieldIdentifiers.Qualifications);
        public string SkillsClass => GetFieldIdentifierCssClass(VacancyReview.FieldIdentifiers.Skills);
        public string ShortDescriptionClass => GetFieldIdentifierCssClass(VacancyReview.FieldIdentifiers.ShortDescription);
        public string ThingsToConsiderClass => GetFieldIdentifierCssClass(VacancyReview.FieldIdentifiers.ThingsToConsider);
        public string TitleClass => GetFieldIdentifierCssClass(VacancyReview.FieldIdentifiers.Title);
        public string TrainingClass => GetFieldIdentifierCssClass(VacancyReview.FieldIdentifiers.Training);
        public string TrainingDescriptionClass => GetFieldIdentifierCssClass(VacancyReview.FieldIdentifiers.TrainingDescription);
        public string TrainingLevelClass => GetFieldIdentifierCssClass(VacancyReview.FieldIdentifiers.TrainingLevel);
        public string VacancyDescriptionClass => GetFieldIdentifierCssClass(VacancyReview.FieldIdentifiers.VacancyDescription);
        public string WageClass => GetFieldIdentifierCssClass(VacancyReview.FieldIdentifiers.Wage);
        public string WorkingWeekClass => GetFieldIdentifierCssClass(VacancyReview.FieldIdentifiers.WorkingWeek);

        private string GetFieldIdentifierCssClass(string fieldIdentifer)
        {
            return FieldIdentifiers.Single(f => f.FieldIdentifier == fieldIdentifer).FieldValueHasChanged ? CssFieldChanged : null;
        }
    }
}